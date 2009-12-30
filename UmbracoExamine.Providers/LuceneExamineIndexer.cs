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
    /// TODO: Need to add support for indexing NON-published nodes (i.e. don't query from cache!)
    /// </summary>
    public class LuceneExamineIndexer : BaseIndexProvider
    {
        public LuceneExamineIndexer() : base() { }
        public LuceneExamineIndexer(IIndexCriteria indexerData) : base(indexerData) { }

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
            LuceneIndexFolder = ExamineLuceneIndexes.Instance.Sets[IndexSetName].IndexDirectory;
            IndexQueueItemFolder = new DirectoryInfo(Path.Combine(LuceneIndexFolder.FullName, "Queue"));

            //check if there's a flag specifying to support unpublished content,
            //if not, set to false;
            bool supportUnpublished;
            if (config["supportUnpublished"] != null && bool.TryParse(config["supportUnpublished"], out supportUnpublished))
                SupportUnpublishedContent = supportUnpublished;
            else
                SupportUnpublishedContent = false;

            if (config["debug"] != null)
                bool.TryParse(config["debug"], out m_ThrowExceptions);  

            //optimize the index async
            ThreadPool.QueueUserWorkItem(
                delegate
                {
                    OptimizeIndex();
                });
        }

        private bool m_ThrowExceptions = false;

        /// <summary>
        /// Used to store a non-tokenized key for the document
        /// </summary>
        public const string IndexTypeFieldName = "__IndexType";

        /// <summary>
        /// Used to store a non-tokenized type for the document
        /// </summary>
        public const string IndexNodeIdFieldName = "__NodeId";

        public DirectoryInfo LuceneIndexFolder { get; protected set; }
        public DirectoryInfo IndexQueueItemFolder { get; protected set; }

        protected string IndexSetName { get; set; }

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

        protected override void OnIndexingError(IndexingErrorEventArgs e)
        {
            if (m_ThrowExceptions)
                throw new Exception("Indexing Error Occurred: " + e.Message, e.InnerException);

            AddLog(e.NodeId, e.Message + ". INNER EXCEPTION: " + e.InnerException.Message, LogTypes.Error);
            base.OnIndexingError(e);
        }

        protected override void OnNodeIndexed(IndexingNodeEventArgs e)
        {
            AddLog(e.NodeId, string.Format("Index created for node. ({0})", LuceneIndexFolder.FullName), LogTypes.System);
            base.OnNodeIndexed(e);
        }

        protected override void OnNodeIndexDeleted(IndexingNodeEventArgs e)
        {
            AddLog(e.NodeId, string.Format("Index deleted for node ({0})", LuceneIndexFolder.FullName), LogTypes.System);
            base.OnNodeIndexDeleted(e);
        }

        #region Provider implementation

        public override bool SupportUnpublishedContent { get; protected set; }

        public override void ReIndexNode(XElement node, IndexType type)
        {
            DeleteFromIndex(node);
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
                    OnIndexingError(new IndexingErrorEventArgs("Cannot rebuild index, the index is currently locked", -1, new Exception()));
                    return;
                }

                //create the writer (this will overwrite old index files)
                writer = new IndexWriter(LuceneIndexFolder.FullName, new StandardAnalyzer(), true);
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("An error occured recreating the index set", -1, ex));
                return;
            }
            finally
            {
                TryWriterClose(ref writer);   
            }

            IndexAll(IndexType.Content);
            IndexAll(IndexType.Media);
        }

        public override void DeleteFromIndex(XElement node)
        {
            var id = (string)node.Attribute("id");
            DeleteFromIndex(new Term(IndexNodeIdFieldName, id));
        }

        /// <summary>
        /// Removes the specified term from the index
        /// </summary>
        /// <param name="indexTerm"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void DeleteFromIndex(Term indexTerm)
        {
            int nodeId = -1;
            if (indexTerm.Field() == "id")
                int.TryParse(indexTerm.Text(), out nodeId);

            IndexReader ir = null;
            try
            {
                VerifyFolder(LuceneIndexFolder);

                //if the index doesn't exist, then no don't attempt to open it.
                if (!IndexExists())
                    return;

                ir = IndexReader.Open(LuceneIndexFolder.FullName);
                int delCount = ir.DeleteDocuments(indexTerm);

                OnNodeIndexDeleted(new IndexingNodeEventArgs(nodeId));
                return;
            }
            catch (Exception ee)
            {
                OnIndexingError(new IndexingErrorEventArgs("Error deleting Lucene index", nodeId, ee));
                return;
            }
            finally
            {
                if (ir != null)
                    ir.Close();
            }
        }

        /// <summary>
        /// Re-indexes all data for the index type specified
        /// </summary>
        /// <param name="type"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void IndexAll(IndexType type)
        {
            //we'll need to remove the type from the index first
            DeleteFromIndex(new Term(IndexTypeFieldName, type.ToString()));

            IndexWriter writer = null;
            try
            {
                //ensure the folder exists
                VerifyFolder(LuceneIndexFolder);

                //check if the index exists and it's locked
                if (IndexExists() && !IndexReady())
                {
                    OnIndexingError(new IndexingErrorEventArgs("Cannot index node, the index is currently locked", -1, new Exception()));
                    return;
                }

                //create the writer (this will overwrite old index files)
                writer = new IndexWriter(LuceneIndexFolder.FullName, new StandardAnalyzer(), !IndexExists());

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

                AddNodesToIndex(xPath, writer, type);

                //raise the completed event, the data returned is irrelevant.
                OnNodesIndexed(new IndexingNodesEventArgs(IndexerData, xPath, type));

                //TODO: This throws an error when multi-publishing... why? should be caught no?
                //based on the info here:
                //http://www.gossamer-threads.com/lists/lucene/java-dev/47895
                //http://lucene.apache.org/java/2_2_0/api/org/apache/lucene/index/IndexWriter.html
                //it is best to only call optimize when there is no activity. 
                //I'll move optimized to be called either on app startup!.. in async
                //writer.Optimize();
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("An error occured recreating the index set", -1, ex));
            }
            finally
            {
                TryWriterClose(ref writer);   
            }
        }

        
        #endregion

        #region Public
        /// <summary>
        /// Adds single node to index. If the node already exists, a duplicate will probably be created. To re-index, use the ReIndex method.
        /// </summary>
        /// <param name="nodeID"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddSingleNodeToIndex(XElement node, IndexType type)
        {
            int nodeId = -1;
            int.TryParse((string)node.Attribute("id"), out nodeId);
            if (nodeId <= 0)
                return;

            IndexWriter writer = null;
            try
            {
                VerifyFolder(LuceneIndexFolder);

                //check if the index doesn't exist, and if so, create it and reindex everything
                if (!IndexExists())
                    IndexAll(type);

                //check if the index is ready to be written to.
                if (!IndexReady())
                {
                    OnIndexingError(new IndexingErrorEventArgs("Cannot index node, the index is currently locked", nodeId, new Exception()));
                    return;
                }

                //XPathNodeIterator umbXml = GetNodeIterator(nodeID, type);
                //XDocument xDoc = umbXml.ToXDocument();
                //var rootNode = xDoc.Elements().First();
                if (!ValidateDocument(node))
                {
                    OnIgnoringNode(new IndexingNodeDataEventArgs(node, null, nodeId));
                    return;
                }

                writer = new IndexWriter(LuceneIndexFolder.FullName, new StandardAnalyzer(), !IndexExists());
                AddDocument(GetDataToIndex(node), writer, nodeId, type);

                //TODO: This throws an error when multi-publishing... why? should be caught no?
                //based on the info here:
                //http://www.gossamer-threads.com/lists/lucene/java-dev/47895
                //http://lucene.apache.org/java/2_2_0/api/org/apache/lucene/index/IndexWriter.html
                //it is best to only call optimize when there is no activity. 
                //I'll move optimized to be called either on app startup!.. in async
                //writer.Optimize();
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("Error deleting Lucene index", nodeId, ex));
            }
            finally
            {
                TryWriterClose(ref writer);
            }
        }


        /// <summary>
        /// This wil optimize the index for searching, this gets executed when this class instance is instantiated.
        /// </summary>
        /// <param name="type"></param>
        /// <remarks>
        /// This can be an expensive operation and should only be called when there is no indexing activity
        /// </remarks>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void OptimizeIndex()
        {
            IndexWriter writer = null;
            try
            {
                VerifyFolder(LuceneIndexFolder);

                //check if the index doesn't exist, and if so, create it and reindex everything
                if (!IndexExists())
                    return;

                //check if the index is ready to be written to.
                if (!IndexReady())
                {
                    OnIndexingError(new IndexingErrorEventArgs("Cannot optimize index, the index is currently locked", -1, new Exception()));
                    return;
                }

                writer = new IndexWriter(LuceneIndexFolder.FullName, new StandardAnalyzer(), !IndexExists());

                //based on the info here:
                //http://www.gossamer-threads.com/lists/lucene/java-dev/47895
                //http://lucene.apache.org/java/2_2_0/api/org/apache/lucene/index/IndexWriter.html
                //it is best to only call optimize when there is no activity. 
                writer.Optimize();
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("Error optmizing Lucene index", -1, ex));
            }
            finally
            {
                TryWriterClose(ref writer);
            }
        } 
        #endregion

        #region Protected
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
            if (!SupportUnpublishedContent && IsProtected(nodeId, node.Attribute("path").Value))
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
            //TODO: Implement this instead of AddDocument then add the documents based on the file queue!
            //SaveIndexQueueItem(fields, nodeId);

            Document d = new Document();
            //add all of our fields to the document index individally            
            fields.ToList().ForEach(x => 
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

        protected void SaveIndexQueueItem(Dictionary<string, string> fields, int nodeId)
        {
            VerifyFolder(IndexQueueItemFolder);
            SerializableDictionary<string, string> sd = new SerializableDictionary<string, string>();
            fields.ToList().ForEach(x =>
            {
                sd.Add(x.Key, x.Value);
            });
            XmlSerializer xs = new XmlSerializer(sd.GetType());
            string output = "";
            using (StringWriter sw = new StringWriter())
            {
                xs.Serialize(sw, sd);
                output = sw.ToString();
                sw.Close();
            }
            FileInfo fi = new FileInfo(Path.Combine(IndexQueueItemFolder.FullName, nodeId.ToString() + "-" + Guid.NewGuid().ToString("N") + ".xml"));
            using (var fileWriter = fi.CreateText())
            {
                fileWriter.Write(output);
                fileWriter.Close();
            }            
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


        #endregion

        #region Private

        

        private void TryWriterClose(ref IndexWriter writer)
        {

            //TODO: MAKE THIS WORK!!!!!!!!!!!!!!!!!!!!

            if (writer != null)
            {                
                //set timer to re-try close
                //uint loops = 0;
                //while (!IndexReady())
                //{
                //    if ((++loops % 100) == 0)
                //    {
                //        OnIndexingError(new IndexingErrorEventArgs("Cannot close/flush indexing documents, index is locked with too many thread cycles completed.", -1, null));
                //        writer = null;                        
                //    }
                //    else
                //    {
                //        Thread.Sleep(1000);
                //    }
                //}
                writer.Close();
            }
        }

        /// <summary>
        /// Adds all nodes with the given xPath root.
        /// </summary>
        /// <param name="xPath"></param>
        /// <param name="writer"></param>
        private void AddNodesToIndex(string xPath, IndexWriter writer, IndexType type)
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
                    AddDocument(GetDataToIndex(node), writer, int.Parse(node.Attribute("id").Value), type);
            }

        }

        /// <summary>
        /// Creates the folder if it does not exist.
        /// </summary>
        /// <param name="folder"></param>
        private void VerifyFolder(DirectoryInfo folder)
        {
            if (!folder.Exists)
                folder.Create();
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
        /// If the index doesn't exist, then create it AND re=index everything.
        /// </summary>
        /// <returns></returns>
        private bool IndexExists()
        {
            return (IndexReader.IndexExists(LuceneIndexFolder.FullName));
        }

        #endregion
    }
}
