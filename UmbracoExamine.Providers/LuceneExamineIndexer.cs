using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoExamine.Core;
using UmbracoExamine.Providers.Config;
using System.IO;
using umbraco.BusinessLogic;
using Lucene.Net.Index;
using System.Xml.Linq;
using System.Xml.XPath;
using Lucene.Net.Documents;
using System.Runtime.CompilerServices;
using Lucene.Net.Analysis.Standard;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic;
using System.Xml;
using System.Threading;
using System.Xml.Serialization;



namespace UmbracoExamine.Providers
{

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Some links picked up along the way:
    /// 
    /// A matrix of concurrent lucene operations: 
    /// http://www.jguru.com/faq/view.jsp?EID=913302.
    /// 
    /// based on the info here, it is best to only call optimize when there is no activity,
    /// we only optimized after the queue has been processed and at startup:
    /// http://www.gossamer-threads.com/lists/lucene/java-dev/47895
    /// http://lucene.apache.org/java/2_2_0/api/org/apache/lucene/index/IndexWriter.html
    /// </remarks>
    public class LuceneExamineIndexer : BaseIndexProvider
    {
        #region Constructors
        public LuceneExamineIndexer()
            : base()
        {
            m_FileWatcher_ElapsedEventHandler = new System.Timers.ElapsedEventHandler(m_FileWatcher_Elapsed);
        }
        public LuceneExamineIndexer(IIndexCriteria indexerData)
            : base(indexerData)
        {
            m_FileWatcher_ElapsedEventHandler = new System.Timers.ElapsedEventHandler(m_FileWatcher_Elapsed);
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Set up all properties for the indexer based on configuration information specified. This will ensure that
        /// all of the folders required by the indexer are created and exist. This will also create an instruction
        /// file declaring the computer name that is part taking in the indexing. This file will then be used to
        /// determine the master indexer machine in a load balanced environment (if one exists).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            //need to check if the index set is specified
            if (config["indexSet"] == null && IndexerData == null)
                throw new ArgumentNullException("indexSet on LuceneExamineIndexer provider has not been set in configuration and/or the IndexerData property has not been explicitly set");

            if (ExamineLuceneIndexes.Instance.Sets[config["indexSet"]] == null)
                throw new ArgumentException("The indexSet specified for the LuceneExamineIndexer provider does not exist");

            IndexSetName = config["indexSet"];

            //get the index criteria
            IndexerData = ExamineLuceneIndexes.Instance.Sets[IndexSetName].ToIndexCriteria();

            //get the folder to index
            LuceneIndexFolder = new DirectoryInfo(Path.Combine(ExamineLuceneIndexes.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));
            IndexQueueItemFolder = new DirectoryInfo(Path.Combine(ExamineLuceneIndexes.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Queue"));


            try
            {
                //ensure all of the folders are created at startup
                VerifyFolder(ExamineLuceneIndexes.Instance.Sets[IndexSetName].IndexDirectory);
                VerifyFolder(LuceneIndexFolder);
                VerifyFolder(IndexQueueItemFolder);
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("Cannot initialize indexer, an error occurred verifying all index folders", -1, ex));
                return;
            }


            //check if there's a flag specifying to support unpublished content,
            //if not, set to false;
            bool supportUnpublished;
            if (config["supportUnpublished"] != null && bool.TryParse(config["supportUnpublished"], out supportUnpublished))
                SupportUnpublishedContent = supportUnpublished;
            else
                SupportUnpublishedContent = false;


            //check if there's a flag specifying to support protected content,
            //if not, set to false;
            bool supportProtected;
            if (config["supportProtected"] != null && bool.TryParse(config["supportProtected"], out supportProtected))
                SupportProtectedContent = supportProtected;
            else
                SupportProtectedContent = false;


            if (config["debug"] != null)
                bool.TryParse(config["debug"], out m_ThrowExceptions);

            ExecutiveIndex = new IndexerExecutive(ExamineLuceneIndexes.Instance.Sets[IndexSetName].IndexDirectory);

            ExecutiveIndex.Initialize();

            //log some info if executive indexer
            if (ExecutiveIndex.IsExecutiveMachine)
            {
                AddLog(-1, string.Format("{0} machine is the Executive Indexer with {1} servers in the cluster",
                    ExecutiveIndex.ExecutiveIndexerMachineName,
                    ExecutiveIndex.ServerCount), LogTypes.Custom);
            }
            

            //optimize the index async
            ThreadPool.QueueUserWorkItem(
                delegate
                {
                    OptimizeIndex();
                });
        } 
        #endregion

        #region Constants & Fields

        private bool m_ThrowExceptions = false;

        /// <summary>
        /// Used to store a non-tokenized key for the document
        /// </summary>
        public const string IndexTypeFieldName = "__IndexType";

        /// <summary>
        /// Used to store a non-tokenized type for the document
        /// </summary>
        public const string IndexNodeIdFieldName = "__NodeId";

        /// <summary>
        /// Used to perform thread locking
        /// </summary>
        private readonly object m_IndexerLocker = new object();

        /// <summary>
        /// used to thread lock calls for creating and verifying folders
        /// </summary>
        private readonly object m_FolderLocker = new object();

        private System.Timers.Timer m_FileWatcher = null;
        private System.Timers.ElapsedEventHandler m_FileWatcher_ElapsedEventHandler;
        
        #endregion

        #region Properties
        public DirectoryInfo LuceneIndexFolder { get; protected set; }

        public DirectoryInfo IndexQueueItemFolder { get; protected set; }

        /// <summary>
        /// The Executive to determine if this is the master indexer
        /// </summary>
        protected IndexerExecutive ExecutiveIndex { get; private set; }

        protected string IndexSetName { get; set; }

        /// <summary>
        /// By default this is false, if set to true then the indexer will include indexing content that is flagged as publicly protected.
        /// This property is ignored if SupportUnpublishedContent is set to true.
        /// </summary>
        public bool SupportProtectedContent { get; protected set; }

        #endregion

        #region Event handlers
        protected override void OnIndexingError(IndexingErrorEventArgs e)
        {
            if (m_ThrowExceptions)
            {
                throw new Exception("Indexing Error Occurred: " + e.Message, e.InnerException);
            }


            AddLog(e.NodeId, e.Message + ". INNER EXCEPTION: " + e.InnerException.Message, LogTypes.Error);
            base.OnIndexingError(e);
        }

        protected override void OnNodeIndexed(IndexingNodeEventArgs e)
        {
            AddLog(e.NodeId, string.Format("Index created for node. ({0})", LuceneIndexFolder.FullName), LogTypes.Custom);
            base.OnNodeIndexed(e);
        }

        protected override void OnIndexDeleted(DeleteIndexEventArgs e)
        {
            AddLog(-1, string.Format("Index deleted for term: {0} with value {1}", e.DeletedTerm.Key, e.DeletedTerm.Value), LogTypes.Custom);
            base.OnIndexDeleted(e);
        }

        #endregion

        #region Provider implementation

        public override bool SupportUnpublishedContent { get; protected set; }

        public override void ReIndexNode(XElement node, IndexType type)
        {
            //first delete the index for the node
            var id = (string)node.Attribute("id");
            SaveDeleteIndexQueueItem(new KeyValuePair<string, string>(IndexNodeIdFieldName, id));
            //now index the single node
            AddSingleNodeToIndex(node, type);
        }

        /// <summary>
        /// Rebuilds the entire index from scratch for all index types
        /// </summary>
        /// <remarks>This will completely delete the index and recrete it</remarks>
        public override void RebuildIndex()
        {
            IndexWriter writer = null;
            try
            {
                //ensure the folder exists
                VerifyFolder(LuceneIndexFolder);

                //check if the index exists and it's locked
                if (IndexExists() && !IndexReady())
                {
                    OnIndexingError(new IndexingErrorEventArgs("Cannot rebuild index, the index is currently locked", -1, null));
                    return;
                }

                //create the writer (this will overwrite old index files)
                writer = new IndexWriter(LuceneIndexFolder.FullName, new StandardAnalyzer(), true);

                //need to remove the queue as we're rebuilding from scratch
                IndexQueueItemFolder.ClearFiles();
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("An error occured recreating the index set", -1, ex));
                return;
            }
            finally
            {
                CloseWriter(ref writer);   
            }

            IndexAll(IndexType.Content);
            IndexAll(IndexType.Media);
        }

        /// <summary>
        /// Deletes a node from the index
        /// </summary>
        /// <param name="node"></param>
        public override void DeleteFromIndex(XElement node)
        {
            //create the queue item to be deleted
            var id = (string)node.Attribute("id");
            SaveDeleteIndexQueueItem(new KeyValuePair<string, string>(IndexNodeIdFieldName, id));
        }

        /// <summary>
        /// Re-indexes all data for the index type specified
        /// </summary>
        /// <param name="type"></param>
        public override void IndexAll(IndexType type)
        {
            //check if the index doesn't exist, and if so, create it and reindex everything, this will obviously index this
            //particular node
            if (!IndexExists())
            {
                RebuildIndex();
                return;
            }
            else
            {
                //create a deletion queue item to remove all items of the specified index type
                SaveDeleteIndexQueueItem(new KeyValuePair<string, string>(IndexTypeFieldName, type.ToString()));
            }
               
            string xPath = "//node[{0}]";
            StringBuilder sb = new StringBuilder();

            //create the xpath statement to match node type aliases if specified
            if (IndexerData.IncludeNodeTypes.Count() > 0)
            {
                sb.Append("(");
                foreach (string field in IndexerData.IncludeNodeTypes)
                {
                    string nodeTypeAlias = "@nodeTypeAlias='{0}'";
                    sb.Append(string.Format(nodeTypeAlias, field));
                    sb.Append(" or ");
                }
                sb.Remove(sb.Length - 4, 4); //remove last " or "
                sb.Append(")");
            }

            //create the xpath statement to match all children of the current node.
            if (IndexerData.ParentNodeId.HasValue)
            {
                if (sb.Length > 0)
                    sb.Append(" and ");
                sb.Append("(");
                //contains(@path, ',1234,')
                sb.Append("contains(@path, '," + IndexerData.ParentNodeId.Value.ToString() + ",')"); //if the path contains comma - id - comma then the nodes must be a child
                sb.Append(")");
            }

            //create the full xpath statement to match the appropriate nodes
            xPath = string.Format(xPath, sb.ToString());

            //in case there are no filters:
            xPath = xPath.Replace("[]", "");

            //raise the event and set the xpath statement to the value returned
            xPath = OnNodesIndexing(new IndexingNodesEventArgs(IndexerData, xPath, type));

            AddNodesToIndex(xPath, type);                            
        }
        
        #endregion

        #region Protected

        /// <summary>
        /// Adds single node to index. If the node already exists, a duplicate will probably be created, 
        /// To re-index, use the ReIndexNode method.
        /// </summary>
        /// <param name="nodeID"></param>
        protected void AddSingleNodeToIndex(XElement node, IndexType type)
        {
            int nodeId = -1;
            int.TryParse((string)node.Attribute("id"), out nodeId);
            if (nodeId <= 0)
                return;

            //check if the index doesn't exist, and if so, create it and reindex everything, this will obviously index this
            //particular node
            if (!IndexExists())
            {
                RebuildIndex();
                return;
            }

            if (!ValidateDocument(node))
            {
                OnIgnoringNode(new IndexingNodeDataEventArgs(node, null, nodeId));
                return;
            }

            //save the index item to a queue file
            SaveAddIndexQueueItem(GetDataToIndex(node), nodeId, type);

            //run the indexer on all queued files
            SafelyProcessQueueItems();

        }

        /// <summary>
        /// This wil optimize the index for searching, this gets executed when this class instance is instantiated.
        /// </summary>
        /// <param name="type"></param>
        /// <remarks>
        /// This can be an expensive operation and should only be called when there is no indexing activity
        /// </remarks>
        protected void OptimizeIndex()
        {
            //check if this machine is the executive.
            if (!ExecutiveIndex.IsExecutiveMachine)
                return;

            lock (m_IndexerLocker)
            {
                IndexWriter writer = null;
                try
                {
                    VerifyFolder(LuceneIndexFolder);

                    if (!IndexExists())
                        return;

                    //check if the index is ready to be written to.
                    if (!IndexReady())
                    {
                        OnIndexingError(new IndexingErrorEventArgs("Cannot optimize index, the index is currently locked", -1, null));
                        return;
                    }

                    writer = new IndexWriter(LuceneIndexFolder.FullName, new StandardAnalyzer(), !IndexExists());

                    writer.Optimize();
                }
                catch (Exception ex)
                {
                    OnIndexingError(new IndexingErrorEventArgs("Error optmizing Lucene index", -1, ex));
                }
                finally
                {
                    CloseWriter(ref writer);
                } 
            }
        } 

        /// <summary>
        /// Removes the specified term from the index
        /// </summary>
        /// <param name="indexTerm"></param>
        /// <returns>Boolean if it successfully deleted the term, or there were on errors</returns>
        protected bool DeleteFromIndex(Term indexTerm, IndexReader ir)
        {
            int nodeId = -1;
            if (indexTerm.Field() == "id")
                int.TryParse(indexTerm.Text(), out nodeId);

            try
            {
                VerifyFolder(LuceneIndexFolder);

                //if the index doesn't exist, then no don't attempt to open it.
                if (!IndexExists())
                    return true;

                int delCount = ir.DeleteDocuments(indexTerm);
                if (delCount > 0)
                {
                    OnIndexDeleted(new DeleteIndexEventArgs(new KeyValuePair<string, string>(indexTerm.Field(), indexTerm.Text()), delCount));
                }                
                return true;
            }
            catch (Exception ee)
            {
                OnIndexingError(new IndexingErrorEventArgs("Error deleting Lucene index", nodeId, ee));
                return false;
            }            
        }

        /// <summary>
        /// Ensures that the node being indexed is of a correct type and is a descendent of the parent id specified.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual bool ValidateDocument(XElement node)
        {
            //check if this document is of a correct type of node type alias
            if (IndexerData.IncludeNodeTypes.Count() > 0)
                if (!IndexerData.IncludeNodeTypes.Contains((string)node.Attribute("nodeTypeAlias")))
                    return false;

            //if this node type is part of our exclusion list, do not validate
            if (IndexerData.ExcludeNodeTypes.Count() > 0)
                if (IndexerData.ExcludeNodeTypes.Contains((string)node.Attribute("nodeTypeAlias")))
                    return false;

            //check if this document is a descendent of the parent
            if (IndexerData.ParentNodeId.HasValue)
                if (!((string)node.Attribute("path")).Contains("," + IndexerData.ParentNodeId.Value.ToString() + ","))
                    return false;

            return true;
        }

        /// <summary>
        /// Collects all of the data that neesd to be indexed as defined in the index set.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual Dictionary<string, string> GetDataToIndex(XElement node)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();

            int nodeId = int.Parse(node.Attribute("id").Value);

            // Test for access if we're only indexing published content
            // return nothing if we're not supporting protected content and it is protected, and we're not supporting unpublished content
            if (!SupportUnpublishedContent && (!SupportProtectedContent && IsProtected(nodeId, node.Attribute("path").Value)))
                return values;

            // Get all user data that we want to index and store into a dictionary 
            foreach (string fieldName in IndexerData.UserFields)
            {
                // Get the value of the data                
                string value = node.UmbSelectDataValue(fieldName);
                //raise the event and assign the value to the returned data from the event
                value = OnGatheringFieldData(new IndexingFieldDataEventArgs(node, fieldName, value, false, nodeId));
                if (!string.IsNullOrEmpty(value))
                    values.Add(fieldName, umbraco.library.StripHtml(value));
            }

            // Add umbraco node properties 
            foreach (string fieldName in IndexerData.UmbracoFields)
            {
                string val = (string)node.Attribute(fieldName);
                val = OnGatheringFieldData(new IndexingFieldDataEventArgs(node, fieldName, val, true, nodeId));
                values.Add(fieldName, val);
            }

            //raise the event and assign the value to the returned data from the event
            values = OnGatheringNodeData(new IndexingNodeDataEventArgs(node, values, nodeId));

            return values;
        }

        /// <summary>
        /// Collects the data for the fields and adds the document.
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="writer"></param>
        /// <param name="nodeId"></param>
        protected virtual void AddDocument(Dictionary<string, string> fields, IndexWriter writer, int nodeId, IndexType type)
        {
            
            Document d = new Document();
            //add all of our fields to the document index individally, don't include the special fields if they exists            
            fields
                .Where(x => x.Key != IndexNodeIdFieldName && x.Key != IndexTypeFieldName)
                .ToList()
                .ForEach(x => 
                {
                    var policy = UmbracoFieldPolicies.GetPolicy(x.Key);
                    d.Add(
                        new Field(x.Key, GetFieldValue(x.Value),
                            Field.Store.YES,
                            policy,
                            policy == Field.Index.NO ? Field.TermVector.NO : Field.TermVector.YES));
                });

            //we want to store the nodeId seperately as it's the index
            d.Add(new Field(IndexNodeIdFieldName, nodeId.ToString(), Field.Store.YES, Field.Index.NO_NORMS, Field.TermVector.NO));
            //add the index type first
            d.Add(new Field(IndexTypeFieldName, type.ToString(), Field.Store.YES, Field.Index.NO_NORMS, Field.TermVector.NO));

            writer.AddDocument(d);

            OnNodeIndexed(new IndexingNodeEventArgs(nodeId));
        }

        /// <summary>
        /// Process all of the queue items. This checks if this machine is the Executive and if it's in a load balanced
        /// environments. If then acts accordingly: 
        ///     Not the executive = doesn't index, i
        ///     In Load Balanced environment = use file watcher timer
        /// </summary>
        protected void SafelyProcessQueueItems()
        {
            //if this is not the master indexer, exit
            if (!ExecutiveIndex.IsExecutiveMachine)
                return;

            //if this is in a Load Balanced environment, then 
            //we will rely on a File system watcher to do the indexing, 
            //otherwise, simply process the queue items
            if (ExecutiveIndex.IsLoadBalancedEnvironment)
            {
                InitializeFileWatcherTimer();
            }
            else
            {
                ForceProcessQueueItems();
            }
            
            
        }

        /// <summary>
        /// Loop through all files in the queue item folder and index them.
        /// Regardless of weather this machine is the executive indexer or not or is in a load balanced environment
        /// or not, this WILL attempt to process the queue items into the index.
        /// </summary>
        /// <returns>
        /// The number of queue items processed
        /// </returns>
        /// <remarks>
        /// Inheritors should be very carefly using this method, SafelyProcessQueueItems will ensure
        /// that the correct machine processes the items into the index. SafelyQueueItems calls this method
        /// if it confirms that this machine is the one to process the queue.
        /// </remarks>
        protected int ForceProcessQueueItems()
        {
            try
            {
                VerifyFolder(LuceneIndexFolder);
                VerifyFolder(IndexQueueItemFolder);
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("Cannot index queue items, an error occurred verifying index folders", -1, ex));
                return 0;
            }

            if (!IndexExists())
            {
                //this shouldn't happen!
                OnIndexingError(new IndexingErrorEventArgs("Cannot index queue items, the index doesn't exist!", -1, null));
                return 0;
            }

            //check if the index is ready to be written to.
            if (!IndexReady())
            {
                OnIndexingError(new IndexingErrorEventArgs("Cannot index queue items, the index is currently locked", -1, null));
                return 0;
            }

            lock (m_IndexerLocker)
            {
                IndexWriter writer = null;
                IndexReader reader = null;

                //track all of the nodes indexed
                var indexedNodes = new List<IndexedNode>();

                try
                {

                    //iterate through all files to add or delete to the index and index the content
                    //and order by time created as to process them in order
                    foreach (var x in IndexQueueItemFolder.GetFiles()
                        .Where(x => x.Extension == ".del" || x.Extension == ".add")
                        .OrderBy(x => x.CreationTime)
                        .ToList())
                    {
                        if (x.Extension == ".del")
                        {
                            if (GetExclusiveIndexReader(ref reader, ref writer))
                            {
                                ProcessDeleteQueueItem(x, reader);
                            }
                            else
                            {
                                OnIndexingError(new IndexingErrorEventArgs("Error indexing queue items, failed to obtain exclusive reader lock", -1, null));
                                return indexedNodes.Count;
                            }
                        }
                        else if (x.Extension == ".add")
                        {
                            if (GetExclusiveIndexWriter(ref writer, ref reader))
                            {
                                ProcessAddQueueItem(x, writer);
                            }
                            else
                            {
                                OnIndexingError(new IndexingErrorEventArgs("Error indexing queue items, failed to obtain exclusive writer lock", -1, null));
                                return indexedNodes.Count;
                            }
                        }
                    }

                    //raise the completed event
                    OnNodesIndexed(new IndexedNodesEventArgs(IndexerData, indexedNodes));

                }
                catch (Exception ex)
                {
                    OnIndexingError(new IndexingErrorEventArgs("Error indexing queue items", -1, ex));
                }
                finally
                {
                    CloseWriter(ref writer);
                    CloseReader(ref reader);
                }

                //optimize index
                OptimizeIndex();

                return indexedNodes.Count;
            }


        }
        
        protected XDocument GetXDocument(string xPath, IndexType type)
        {
            // Get all the nodes of nodeTypeAlias == nodeTypeAlias
            XPathNodeIterator umbXml;
            switch (type)
            {
                case IndexType.Content:
                    if (this.SupportUnpublishedContent)
                    {
                        //This is quite an intensive operation...
                        //get all root content, then get the XML structure for all children,
                        //then run xpath against the navigator that's created
                        var rootContent = umbraco.cms.businesslogic.web.Document.GetRootDocuments();
                        var xmlContent = XDocument.Parse("<content></content>");
                        var xDoc = new XmlDocument();
                        foreach (var c in rootContent)
                        {
                            var xNode = xDoc.CreateNode(XmlNodeType.Element, "node", "");
                            c.XmlPopulate(xDoc, ref xNode, true); 
                            xmlContent.Root.Add(xNode.ToXElement());
                        }
                        umbXml = (XPathNodeIterator)xmlContent.CreateNavigator().Evaluate(xPath);
                        return umbXml.ToXDocument();
                    }
                    else
                    {
                        //If we're only dealing with published content, this is easy
                        return umbraco.library.GetXmlNodeByXPath(xPath).ToXDocument();
                    }
                case IndexType.Media:

                    //This is quite an intensive operation...
                    //get all root media, then get the XML structure for all children,
                    //then run xpath against the navigator that's created
                    Media[] rootMedia = Media.GetRootMedias();
                    var xmlMedia = XDocument.Parse("<media></media>");
                    foreach (var media in rootMedia)
                    {
                        var nodes = umbraco.library.GetMedia(media.Id, true);
                        xmlMedia.Root.Add(XElement.Parse(nodes.Current.OuterXml));

                    }
                    umbXml = (XPathNodeIterator)xmlMedia.CreateNavigator().Evaluate(xPath);
                    return umbXml.ToXDocument();
            }

            return null;
        }

        /// <summary>
        /// Saves a file indicating that the executive indexer should remove the from the index those that match
        /// the term saved in this file.
        /// This will save a file prefixed with the current machine name with an extension of .dev
        /// </summary>
        /// <param name="term"></param>
        protected void SaveDeleteIndexQueueItem(KeyValuePair<string, string> term)
        {
            try
            {
                VerifyFolder(IndexQueueItemFolder);
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("Cannot save index queue item for deletion, an error occurred verifying queue folder", -1, ex));
                return;
            }

            var terms = new Dictionary<string, string>();
            terms.Add(term.Key, term.Value);
            var fileName = Environment.MachineName + "-" + Guid.NewGuid().ToString("N");
            FileInfo fi = new FileInfo(Path.Combine(IndexQueueItemFolder.FullName, fileName + ".del"));
            terms.SaveToDisk(fi);
        }

        /// <summary>
        /// Writes the information for the fields to a file names with the computer's name that is running the index and 
        /// a GUID value. The indexer will then index the values stored in the files in another thread so that processing may continue.
        /// This will save a file prefixed with the current machine name with an extension of .add
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="nodeId"></param>
        protected void SaveAddIndexQueueItem(Dictionary<string, string> fields, int nodeId, IndexType type)
        {
            try
            {
                VerifyFolder(IndexQueueItemFolder);
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("Cannot save index queue item, an error occurred verifying queue folder", nodeId, ex));
                return;
            }

            //ensure the special fields are added to the dictionary to be saved to file
            if (!fields.ContainsKey(IndexNodeIdFieldName)) fields.Add(IndexNodeIdFieldName, nodeId.ToString());
            if (!fields.ContainsKey(IndexTypeFieldName)) fields.Add(IndexTypeFieldName, type.ToString());

            var fileName = Environment.MachineName + "-" + nodeId.ToString() + "-" + Guid.NewGuid().ToString("N");
            FileInfo fi = new FileInfo(Path.Combine(IndexQueueItemFolder.FullName, fileName + ".add"));
            fields.SaveToDisk(fi);
        }

        #endregion

        #region Private

        private void InitializeFileWatcherTimer()
        {
            if (m_FileWatcher != null)
            {
                //if this is not the master indexer anymore... perhaps another server has taken over somehow...
                if (!ExecutiveIndex.IsExecutiveMachine)
                {
                    //stop the timer, remove event handlers and close
                    m_FileWatcher.Stop();
                    m_FileWatcher.Elapsed -= m_FileWatcher_ElapsedEventHandler;
                    m_FileWatcher.Dispose();
                    m_FileWatcher = null;
                }

                return;
            }
         
            m_FileWatcher = new System.Timers.Timer(new TimeSpan(0, 1, 0).TotalMilliseconds);
            m_FileWatcher.Elapsed += m_FileWatcher_ElapsedEventHandler;
            m_FileWatcher.AutoReset = false;
            m_FileWatcher.Start();
        }
       

        /// <summary>
        /// Handles the file watcher timer poll elapsed event
        /// This will:
        /// - Disable the FileSystemWatcher        
        /// - Recursively process all queue items in the folder and check after processing if any more files have been added
        /// - Once there's no more files to be processed, re-enables the watcher
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_FileWatcher_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            //stop the event system
            m_FileWatcher.Stop();
            var numProcessedItems = 0;
            do
            {
                numProcessedItems = ForceProcessQueueItems();
            } while (numProcessedItems > 0);

            //restart the timer.
            m_FileWatcher.Start();
      
        }        

        /// <summary>
        /// Checks the writer passed in to see if it is active, if not, checks if the index is locked. If it is locked, 
        /// returns checks if the reader is not null and tries to close it. if it's still locked returns null, otherwise
        /// creates a new writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        private bool GetExclusiveIndexWriter(ref IndexWriter writer, ref IndexReader reader)
        {
            if (writer != null)
                return true;

            if (!IndexReady())
            {
                if (reader != null)
                {
                    CloseReader(ref reader);
                    if (!IndexReady())
                    {
                        return false;
                    }
                }
            }

            writer = new IndexWriter(LuceneIndexFolder.FullName, new StandardAnalyzer(), false);
            return true;
        }

        /// <summary>
        /// Checks the reader passed in to see if it is active, if not, checks if the index is locked. If it is locked, 
        /// returns checks if the writer is not null and tries to close it. if it's still locked returns null, otherwise
        /// creates a new reader.
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        private bool GetExclusiveIndexReader(ref IndexReader reader, ref IndexWriter writer)
        {
            if (reader != null)
                return true;

            if (!IndexReady())
            {
                if (writer != null)
                {
                    CloseWriter(ref writer);
                    if (!IndexReady())
                    {
                        return false;
                    }
                }
            }

            reader = IndexReader.Open(LuceneIndexFolder.FullName);
            return true;
        }

        /// <summary>
        /// Reads the FileInfo passed in into a dictionary object and deletes it from the index
        /// </summary>
        /// <param name="x"></param>
        private void ProcessDeleteQueueItem(FileInfo x, IndexReader ir)
        {
            //get the dictionary object from the file data
            SerializableDictionary<string, string> sd = new SerializableDictionary<string, string>();
            sd.ReadFromDisk(x);
            
            //we know that there's only ever one item saved to the dictionary for deletions
            if (sd.Count != 1)
            {
                OnIndexingError(new IndexingErrorEventArgs("Could not remove queue item from index, the file is not properly formatted", -1, null));
                return;
            }
            var term = sd.First();
            DeleteFromIndex(new Term(term.Key, term.Value), ir);

            //remove the file
            x.Delete();
        }

        /// <summary>
        /// Reads the FileInfo passed in into a dictionary object and adds it to the index
        /// </summary>
        /// <param name="x"></param>
        private IndexedNode ProcessAddQueueItem(FileInfo x, IndexWriter writer)
        {
            //get the dictionary object from the file data
            SerializableDictionary<string, string> sd = new SerializableDictionary<string, string>();
            sd.ReadFromDisk(x);

            //get the index type
            IndexType indexType = (IndexType)Enum.Parse(typeof(IndexType), sd[IndexTypeFieldName]);
            //get the node id
            int nodeId = int.Parse(sd[IndexNodeIdFieldName]);
            //now, add the index with our dictionary object
            AddDocument(sd.ToDictionary(), writer, nodeId, indexType);

            //remove the file
            x.Delete();

            return new IndexedNode() { NodeId = nodeId, Type = indexType };
        }

        /// <summary>
        /// All field data will be stored into Lucene as is except for dates, these can be stored as standard: yyyyMMdd
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private string GetFieldValue(string val)
        {
            DateTime date;
            if (DateTime.TryParse(val, out date))
            {
                return date.ToString("yyyyMMdd");
            }
            else
                return val;
        }

        /// <summary>
        /// Unfortunately, we need to implement our own IsProtected method since 
        /// the Umbraco core code requires an HttpContext for this method and when we're running
        /// async, there is no context
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        private XmlNode GetPage(int documentId)
        {
            XmlNode x = umbraco.cms.businesslogic.web.Access.AccessXml.SelectSingleNode("/access/page [@id=" + documentId.ToString() + "]");
            return x;
        }

        /// <summary>
        /// Unfortunately, we need to implement our own IsProtected method since 
        /// the Umbraco core code requires an HttpContext for this method and when we're running
        /// async, there is no context
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsProtected(int nodeId, string path)
        {
            foreach (string id in path.Split(','))
            {
                if (GetPage(int.Parse(id)) != null)
                {
                    return true;
                }
            }
            return false;
        }
       
        private void CloseWriter(ref IndexWriter writer)
        {
            if (writer != null)
            {                               
                writer.Close();
                writer = null;
            }
        }

        private void CloseReader(ref IndexReader reader)
        {
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
        }

        /// <summary>
        /// Adds all nodes with the given xPath root.
        /// </summary>
        /// <param name="xPath"></param>
        /// <param name="writer"></param>
        private void AddNodesToIndex(string xPath, IndexType type)
        {
            // Get all the nodes of nodeTypeAlias == nodeTypeAlias
            XDocument xDoc = GetXDocument(xPath, type);
            if (xDoc == null)
                return;

            XElement rootNode = xDoc.Elements().First();

            IEnumerable<XElement> children = rootNode.Elements();

            foreach (XElement node in children)
            {
                if (ValidateDocument(node))
                {
                    //save the index item to a queue file
                    SaveAddIndexQueueItem(GetDataToIndex(node), int.Parse(node.Attribute("id").Value), type);
                }
                    
            }

            //run the indexer on all queued files
            SafelyProcessQueueItems();

        }

        /// <summary>
        /// Creates the folder if it does not exist.
        /// </summary>
        /// <param name="folder"></param>
        private void VerifyFolder(DirectoryInfo folder)
        {
            lock (m_FolderLocker)
            {
                if (!folder.Exists)
                    folder.Create();
            }
        }

        /// <summary>
        /// Checks if the index is ready to open/write to.
        /// </summary>
        /// <returns></returns>
        private bool IndexReady()
        {
            return (!IndexReader.IsLocked(LuceneIndexFolder.FullName));
        }

        /// <summary>
        /// Check if there is an index in the index folder
        /// </summary>
        /// <returns></returns>
        private bool IndexExists()
        {
            return (IndexReader.IndexExists(LuceneIndexFolder.FullName));
        }

        /// <summary>
        /// Adds a log entry to the umbraco log
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="msg"></param>
        /// <param name="type"></param>
        private void AddLog(int nodeId, string msg, LogTypes type)
        {
            Log.Add(type, nodeId, "[UmbracoExamine] " + msg);
        }

        #endregion
    }
}
