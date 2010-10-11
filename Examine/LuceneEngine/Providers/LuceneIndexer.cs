using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Examine;
using Examine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Examine.LuceneEngine.Config;
using Lucene.Net.Util;
using System.ComponentModel;

namespace Examine.LuceneEngine.Providers
{
    public abstract class LuceneIndexer : BaseIndexProvider, IDisposable
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        protected LuceneIndexer()
            : base()
        {
            m_FileWatcher_ElapsedEventHandler = new System.Timers.ElapsedEventHandler(FileWatcher_Elapsed);
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        protected LuceneIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath)
            : base(indexerData)
        {
            m_FileWatcher_ElapsedEventHandler = new System.Timers.ElapsedEventHandler(FileWatcher_Elapsed);

            //set up our folders based on the index path
            LuceneIndexFolder = new DirectoryInfo(Path.Combine(indexPath.FullName, "Index"));
            IndexQueueItemFolder = new DirectoryInfo(Path.Combine(indexPath.FullName, "Queue"));

            ReInitialize();
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Set up all properties for the indexer based on configuration information specified. This will ensure that
        /// all of the folders required by the indexer are created and exist. This will also create an instruction
        /// file declaring the computer name that is part taking in the indexing. This file will then be used to
        /// determine the master indexer machine in a load balanced environment (if one exists).
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The name of the provider is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The name of the provider has a length of zero.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.
        /// </exception>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            //Need to check if the index set or IndexerData is specified...

            if (config["indexSet"] == null && IndexerData == null)
            {
                //if we don't have either, then we'll try to set the index set by naming conventions
                var found = false;
                if (name.EndsWith("Indexer"))
                {
                    var setNameByConvension = name.Remove(name.LastIndexOf("Indexer")) + "IndexSet";
                    //check if we can assign the index set by naming convention
                    var set = IndexSets.Instance.Sets.Cast<IndexSet>()
                        .Where(x => x.SetName == setNameByConvension)
                        .SingleOrDefault();

                    if (set != null)
                    {
                        //we've found an index set by naming conventions :)
                        IndexSetName = set.SetName;

                        //get the index criteria and ensure folder
                        IndexerData = GetIndexerData(IndexSets.Instance.Sets[IndexSetName]);
                        VerifyFolder(IndexSets.Instance.Sets[IndexSetName].IndexDirectory);

                        //now set the index folders
                        LuceneIndexFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));
                        IndexQueueItemFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Queue"));

                        found = true;
                    }
                }

                if (!found)
                    throw new ArgumentNullException("indexSet on LuceneExamineIndexer provider has not been set in configuration and/or the IndexerData property has not been explicitly set");

            }
            else if (config["indexSet"] != null)
            {
                //if an index set is specified, ensure it exists and initialize the indexer based on the set

                if (IndexSets.Instance.Sets[config["indexSet"]] == null)
                {
                    throw new ArgumentException("The indexSet specified for the LuceneExamineIndexer provider does not exist");
                }
                else
                {
                    IndexSetName = config["indexSet"];

                    //get the index criteria and ensure folder
                    IndexerData = GetIndexerData(IndexSets.Instance.Sets[IndexSetName]);
                    VerifyFolder(IndexSets.Instance.Sets[IndexSetName].IndexDirectory);

                    //now set the index folders
                    LuceneIndexFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));
                    IndexQueueItemFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Queue"));
                }
            }

            if (config["analyzer"] != null)
            {
                //this should be a fully qualified type
                var analyzerType = Type.GetType(config["analyzer"]);
                IndexingAnalyzer = (Analyzer)Activator.CreateInstance(analyzerType);
            }
            else
            {
                IndexingAnalyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
            }

            //create our internal searcher, this is useful for inheritors to be able to search their own indexes inside of their indexer
            InternalSearcher = new LuceneSearcher(this.LuceneIndexFolder);
            var searcherConfig = new NameValueCollection();
            searcherConfig.Add("indexSet", this.IndexSetName);
            //We should use the same analyzer for searching and indexing
            searcherConfig.Add("analyzer", IndexingAnalyzer.GetType().AssemblyQualifiedName);
            InternalSearcher.Initialize(Guid.NewGuid().ToString("N"), searcherConfig);

            RunAsync = true;
            if (config["runAsync"] != null)
            {
                RunAsync = bool.Parse(config["runAsync"]);
            }

            IndexSecondsInterval = 30;
            if (config["interval"] != null)
            {
                IndexSecondsInterval = int.Parse(config["interval"]);
            }

            ReInitialize();

            CommitCount = 0;

            OptimizeIndex();
        }

        #endregion

        #region Constants & Fields

        /// <summary>
        /// The prefix characters denoting a special field stored in the lucene index for use internally
        /// </summary>
        public const string SpecialFieldPrefix = "__";

        /// <summary>
        /// The prefix added to a field when it is included in the index for sorting
        /// </summary>
        public const string SortedFieldNamePrefix = "__Sort_";

        /// <summary>
        /// Specifies how many index commits are performed before running an optimization
        /// </summary>
        public const int OptimizationCommitThreshold = 100;

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

        /// <summary>
        /// Used for double check locking during an index operation
        /// </summary>
        private bool m_IsIndexing = false;

        private System.Timers.Timer m_FileWatcher = null;
        private System.Timers.ElapsedEventHandler m_FileWatcher_ElapsedEventHandler;

        /// <summary>
        /// We need an internal searcher used to search against our own index.
        /// This is used for finding all descendant nodes of a current node when deleting indexes.
        /// </summary>
        protected internal BaseSearchProvider InternalSearcher;

        #endregion

        #region Properties

        /// <summary>
        /// The analyzer to use when indexing content, by default, this is set to StandardAnalyzer
        /// </summary>
        public Analyzer IndexingAnalyzer { get; protected internal set; }

        /// <summary>
        /// Used to keep track of how many index commits have been performed.
        /// This is used to determine when index optimization needs to occur.
        /// </summary>
        public int CommitCount { get; protected internal set; }

        /// <summary>
        /// Indicates whether or this system will process the queue items asynchonously. Default is true.
        /// </summary>
        public bool RunAsync { get; protected internal set; }

        /// <summary>
        /// The interval (in seconds) specified for the timer to process index queue items.
        /// This is only relevant if <see cref="RunAsnc"/> is true.
        /// </summary>
        public int IndexSecondsInterval { get; protected internal set; }

        /// <summary>
        /// The folder that stores the Lucene Index files
        /// </summary>
        public DirectoryInfo LuceneIndexFolder { get; protected internal set; }

        /// <summary>
        /// The folder that stores the index queue files
        /// </summary>
        public DirectoryInfo IndexQueueItemFolder { get; protected internal set; }

        /// <summary>
        /// The Executive to determine if this is the master indexer
        /// </summary>
        protected IndexerExecutive ExecutiveIndex { get; set; }

        /// <summary>
        /// The index set name which references an Examine <see cref="IndexSet"/>
        /// </summary>
        public string IndexSetName { get; protected internal set; }

        /// <summary>
        /// Gets the full IndexSet information for this provider
        /// </summary>
        /// <value>The index set.</value>
        public IndexSet IndexSet
        {
            get
            {
                return IndexSets.Instance.Sets[this.IndexSetName];
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when [index optimizing].
        /// </summary>
        public event EventHandler IndexOptimizing;

        /// <summary>
        /// Occurs when [document writing].
        /// </summary>
        public event EventHandler<DocumentWritingEventArgs> DocumentWriting;

        /// <summary>
        /// An event that is triggered when this machine has been elected as the IndexerExecutive
        /// </summary>
        public event EventHandler<IndexerExecutiveAssignedEventArgs> IndexerExecutiveAssigned;

        #endregion

        #region Event handlers

        protected virtual void OnIndexerExecutiveAssigned(IndexerExecutiveAssignedEventArgs e)
        {
            if (IndexerExecutiveAssigned != null)
                IndexerExecutiveAssigned(this, e);
        }

        /// <summary>
        /// Called when an indexing error occurs
        /// </summary>
        /// <param name="e"></param>
        /// <param name="resetIndexingFlag">set to true if the IsIndexing flag should be reset (set to false) so future indexing operations can occur</param>
        protected void OnIndexingError(IndexingErrorEventArgs e, bool resetIndexingFlag)
        {
            if (resetIndexingFlag)
            {
                //reset our volatile flag... something else funny is going on but we don't want this to prevent ALL future operations
                m_IsIndexing = false;
            }

            OnIndexingError(e);
        }

        /// <summary>
        /// Called when an indexing error occurs
        /// </summary>
        /// <param name="e"></param>
        protected override void OnIndexingError(IndexingErrorEventArgs e)
        {
            base.OnIndexingError(e);

            if (!RunAsync)
            {
                throw new Exception("Indexing Error Occurred: " + e.Message, e.InnerException);
            }

        }

        protected virtual void OnDocumentWriting(DocumentWritingEventArgs docArgs)
        {
            if (DocumentWriting != null)
                DocumentWriting(this, docArgs);
        }

        protected virtual void OnIndexOptimizing(EventArgs e)
        {
            if (IndexOptimizing != null)
                IndexOptimizing(this, e);
        }

        /// <summary>
        /// This is here for inheritors to deal with if there's a duplicate entry in the fields dictionary when trying to index.
        /// The system by default just ignores duplicates but this will give inheritors a chance to do something about it (i.e. logging, alerting...)
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="indexSetName"></param>
        /// <param name="fieldName"></param>
        protected virtual void OnDuplicateFieldWarning(int nodeId, string indexSetName, string fieldName) { }

        #endregion

        #region Provider implementation

        /// <summary>
        /// Determines if the manager will call the indexing methods when content is saved or deleted as
        /// opposed to cache being updated.
        /// </summary>
        /// <value></value>
        public override bool SupportUnpublishedContent { get; protected set; }

        /// <summary>
        /// Forces a particular XML node to be reindexed
        /// </summary>
        /// <param name="node">XML node to reindex</param>
        /// <param name="type">Type of index to use</param>
        public override void ReIndexNode(XElement node, string type)
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
        /// <remarks>This will completely delete the index and recreate it</remarks>
        public override void RebuildIndex()
        {
            IndexWriter writer = null;
            try
            {
                //ensure the folder exists
                ReInitialize();

                //check if the index exists and it's locked
                if (IndexExists() && !IndexReady())
                {
                    OnIndexingError(new IndexingErrorEventArgs("Cannot rebuild index, the index is currently locked", -1, null));
                    return;
                }

                //create the writer (this will overwrite old index files)
                writer = new IndexWriter(new SimpleFSDirectory(LuceneIndexFolder), IndexingAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);

                //need to remove the queue as we're rebuilding from scratch
                IndexQueueItemFolder.ClearFiles();
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("An error occurred recreating the index set", -1, ex));
                return;
            }
            finally
            {
                CloseWriter(ref writer);
            }

            //call abstract method
            PerformIndexRebuild();


        }

        /// <summary>
        /// Deletes a node from the index.                
        /// </summary>
        /// <remarks>
        /// When a content node is deleted, we also need to delete it's children from the index so we need to perform a 
        /// custom Lucene search to find all decendents and create Delete item queues for them too.
        /// </remarks>
        /// <param name="nodeId">ID of the node to delete</param>
        public override void DeleteFromIndex(string nodeId)
        {
            //create the queue item to be deleted
            SaveDeleteIndexQueueItem(new KeyValuePair<string, string>(IndexNodeIdFieldName, nodeId));

            //need to process the queue items, otherwise the delete files aren't processed until the next publish
            //if (!RunAsync)
            //    ForceProcessQueueItems();
            SafelyProcessQueueItems();
        }

        /// <summary>
        /// Re-indexes all data for the index type specified
        /// </summary>
        /// <param name="type"></param>
        public override void IndexAll(string type)
        {
            //check if the index doesn't exist, and if so, create it and reindex everything
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

            //now do the indexing...
            PerformIndexAll(type);
        }

        #endregion

        #region Protected

        /// <summary>
        /// This will add all of the nodes defined to the index for the index type. WARNING: if the nodes already exists in the index, this will duplicate them
        /// </summary>
        /// <remarks>
        /// This is used to ADD items to the index in bulk and assumes that these items don't already exist in the index. If they do, you will have duplicates.
        /// </remarks>
        /// <param name="nodes"></param>
        /// <param name="type"></param>
        protected void AddNodesToIndex(IEnumerable<XElement> nodes, string type)
        {
            foreach (XElement node in nodes)
            {
                if (ValidateDocument(node))
                {
                    //save the index item to a queue file
                    var fields = GetDataToIndex(node, type);

                    SaveAddIndexQueueItem(fields, int.Parse(node.Attribute("id").Value), type);
                }

            }

            //run the indexer on all queued files
            SafelyProcessQueueItems();
        }

        /// <summary>
        /// Called to perform the operation to do the actual indexing of an index type after the lucene index has been re-initialized.
        /// </summary>
        /// <param name="type"></param>
        protected abstract void PerformIndexAll(string type);

        /// <summary>
        /// Called to perform the actual rebuild of the indexes once the lucene index has been re-initialized.
        /// </summary>
        protected abstract void PerformIndexRebuild();

        /// <summary>
        /// Returns IIndexCriteria object from the IndexSet
        /// </summary>
        /// <param name="indexSet"></param>
        protected virtual IIndexCriteria GetIndexerData(IndexSet indexSet)
        {
            return new IndexCriteria(
                indexSet.IndexAttributeFields.ToList().Select(x => x.Name).ToArray(),
                indexSet.IndexUserFields.ToList().Select(x => x.Name).ToArray(),
                indexSet.IncludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                indexSet.ExcludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                indexSet.IndexParentId);
        }

        /// <summary>
        /// Checks if the index is ready to open/write to.
        /// </summary>
        /// <returns></returns>
        protected bool IndexReady()
        {
            return (!IndexWriter.IsLocked(new SimpleFSDirectory(LuceneIndexFolder)));
        }

        /// <summary>
        /// Check if there is an index in the index folder
        /// </summary>
        /// <returns></returns>
        protected bool IndexExists()
        {
            return IndexReader.IndexExists(new SimpleFSDirectory(LuceneIndexFolder));
        }

        /// <summary>
        /// Adds single node to index. If the node already exists, a duplicate will probably be created,
        /// To re-index, use the ReIndexNode method.
        /// </summary>
        /// <param name="node">The node to index.</param>
        /// <param name="type">The type to store the node as.</param>
        protected virtual void AddSingleNodeToIndex(XElement node, string type)
        {
            //check if the index doesn't exist, and if so, create it and reindex everything, this will obviously index this
            //particular node
            if (!IndexExists())
            {
                RebuildIndex();
                return;
            }

            int nodeId = -1;
            int.TryParse((string)node.Attribute("id"), out nodeId);
            if (nodeId <= 0)
                return;

            if (!ValidateDocument(node))
            {
                OnIgnoringNode(new IndexingNodeDataEventArgs(node, nodeId, null, type));
                return;
            }

            //save the index item to a queue file
            SaveAddIndexQueueItem(GetDataToIndex(node, type), nodeId, type);

            //run the indexer on all queued files
            SafelyProcessQueueItems();

        }

        /// <summary>
        /// This wil optimize the index for searching, this gets executed when this class instance is instantiated.
        /// </summary>
        /// <remarks>
        /// This can be an expensive operation and should only be called when there is no indexing activity
        /// </remarks>
        protected void OptimizeIndex()
        {
            //check if this machine is the executive.
            if (!ExecutiveIndex.IsExecutiveMachine)
                return;

            if (!m_IsIndexing)
            {
                lock (m_IndexerLocker)
                {
                    //double check
                    if (!m_IsIndexing)
                    {

                        //set our volatile flag
                        m_IsIndexing = true;

                        IndexWriter writer = null;
                        try
                        {
                            VerifyFolder(LuceneIndexFolder);

                            if (!IndexExists())
                                return;

                            //check if the index is ready to be written to.
                            if (!IndexReady())
                            {
                                OnIndexingError(new IndexingErrorEventArgs("Cannot optimize index, the index is currently locked", -1, null), true);
                                return;
                            }

                            writer = new IndexWriter(new SimpleFSDirectory(LuceneIndexFolder), IndexingAnalyzer, !IndexExists(), IndexWriter.MaxFieldLength.UNLIMITED);

                            OnIndexOptimizing(new EventArgs());

                            writer.Optimize();
                        }
                        catch (Exception ex)
                        {
                            OnIndexingError(new IndexingErrorEventArgs("Error optimizing Lucene index", -1, ex));
                        }
                        finally
                        {
                            //set our volatile flag
                            m_IsIndexing = false;

                            CloseWriter(ref writer);
                        }
                    }

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
                ReInitialize();

                //if the index doesn't exist, then no don't attempt to open it.
                if (!IndexExists())
                    return true;

                int delCount = ir.DeleteDocuments(indexTerm);

                ir.Commit(); //commit the changes!

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
                if (!IndexerData.IncludeNodeTypes.Contains(node.ExamineNodeTypeAlias()))
                    return false;

            //if this node type is part of our exclusion list, do not validate
            if (IndexerData.ExcludeNodeTypes.Count() > 0)
                if (IndexerData.ExcludeNodeTypes.Contains(node.ExamineNodeTypeAlias()))
                    return false;

            return true;
        }


        /// <summary>
        /// Translates the XElement structure into a dictionary object to be indexed.
        /// </summary>
        /// <remarks>
        /// This is used when re-indexing an individual node since this is the way the provider model works.
        /// For this provider, it will use a very similar XML structure as umbraco 4.0.x:
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <root>
        ///     <node id="1234" nodeTypeAlias="yourIndexType">
        ///         <data alias="fieldName1">Some data</data>
        ///         <data alias="fieldName2">Some other data</data>
        ///     </node>
        ///     <node id="345" nodeTypeAlias="anotherIndexType">
        ///         <data alias="fieldName3">More data</data>
        ///     </node>
        /// </root>
        /// ]]>
        /// </code>        
        /// </example>
        /// <param name="node"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual Dictionary<string, string> GetDataToIndex(XElement node, string type)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();

            int nodeId = int.Parse(node.Attribute("id").Value);

            // Get all user data that we want to index and store into a dictionary 
            foreach (string fieldName in IndexerData.UserFields)
            {
                // Get the value of the data                
                string value = node.SelectExamineDataValue(fieldName);

                //raise the event and assign the value to the returned data from the event
                var indexingFieldDataArgs = new IndexingFieldDataEventArgs(node, fieldName, value, false, nodeId);
                OnGatheringFieldData(indexingFieldDataArgs);
                value = indexingFieldDataArgs.FieldValue;

                //don't add if the value is empty/null
                if (!string.IsNullOrEmpty(value))
                {
                    if (!string.IsNullOrEmpty(value))
                        values.Add(fieldName, value);
                }
            }

            // Add umbraco node properties 
            foreach (string fieldName in IndexerData.StandardFields)
            {
                string val = node.SelectExaminePropertyValue(fieldName);
                var args = new IndexingFieldDataEventArgs(node, fieldName, val, true, nodeId);
                OnGatheringFieldData(args);
                val = args.FieldValue;

                //don't add if the value is empty/null                
                if (!string.IsNullOrEmpty(val))
                {
                    values.Add(fieldName, val);
                }

            }

            //raise the event and assign the value to the returned data from the event
            var indexingNodeDataArgs = new IndexingNodeDataEventArgs(node, nodeId, values, type);
            OnGatheringNodeData(indexingNodeDataArgs);
            values = indexingNodeDataArgs.Fields;

            return values;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected virtual FieldIndexTypes GetPolicy(string fieldName)
        {
            return FieldIndexTypes.ANALYZED;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldIndex"></param>
        /// <returns></returns>
        private Field.Index TranslateFieldIndexTypeToLuceneType(FieldIndexTypes fieldIndex)
        {
            switch (fieldIndex)
            {
                case FieldIndexTypes.ANALYZED:
                    return Field.Index.ANALYZED;

                case FieldIndexTypes.ANALYZED_NO_NORMS:
                    return Field.Index.ANALYZED_NO_NORMS;

                case FieldIndexTypes.NO:
                    return Field.Index.NO;

                case FieldIndexTypes.NOT_ANALYZED:
                    return Field.Index.NOT_ANALYZED;

                case FieldIndexTypes.NOT_ANALYZED_NO_NORMS:
                    return Field.Index.NOT_ANALYZED_NO_NORMS;

                default:
                    throw new Exception("Unknown field index type");
            }
        }


        /// <summary>
        /// Collects the data for the fields and adds the document which is then committed into Lucene.Net's index
        /// </summary>
        /// <param name="fields">The fields and their associated data.</param>
        /// <param name="writer">The writer that will be used to update the Lucene index.</param>
        /// <param name="nodeId">The node id.</param>
        /// <param name="type">The type to index the node as.</param>
        /// <param name="path">The path of the content node</param>
        /// <remarks>
        /// This will normalize (lowercase) all text before it goes in to the index.
        /// </remarks>
        protected virtual void AddDocument(Dictionary<string, string> fields, IndexWriter writer, int nodeId, string type)
        {
            var args = new IndexingNodeEventArgs(nodeId, fields, type);
            OnNodeIndexing(args);
            if (args.Cancel)
                return;

            Document d = new Document();

            //get all index set fields that are defined
            var indexSetFields = IndexSet.IndexUserFields.ToList().Concat(IndexSet.IndexAttributeFields.ToList());

            //add all of our fields to the document index individually, don't include the special fields if they exists            
            var validFields = fields.Where(x => !x.Key.StartsWith(SpecialFieldPrefix)).ToList();

            foreach (var x in validFields)
            {
                var ourPolicyType = GetPolicy(x.Key);
                var lucenePolicy = TranslateFieldIndexTypeToLuceneType(ourPolicyType);

                var indexedFields = indexSetFields.Where(o => o.Name == x.Key);

                if (indexedFields.Count() == 0)
                {
                    //TODO: Decide if we should support non-strings in here too
                    d.Add(
                    new Field(x.Key,
                        x.Value,
                        Field.Store.YES,
                        lucenePolicy,
                        lucenePolicy == Field.Index.NO ? Field.TermVector.NO : Field.TermVector.YES));
                }

                else
                {
                    //checks if there's duplicates fields, if not check if the field needs to be sortable...
                    if (indexedFields.Count() > 1)
                    {
                        //we wont error if there are two fields which match, we'll just log an error and ignore the 2nd field                        
                        OnDuplicateFieldWarning(nodeId, x.Key, IndexSetName);
                    }
                    else
                    {
                        //TODO: Work out how to do dates properly
                        //Currently we're just pretending it's a string
                        IndexField indexField = indexedFields.First();
                        Fieldable field = null;
                        object parsedVal = null;
                        switch (indexField.Type.ToUpper())
                        {
                            case "NUMBER":
                            case "INT":
                                if (!TryConvert<int>(x.Value, out parsedVal))
                                    break;
                                field = new NumericField(x.Key, Field.Store.YES, lucenePolicy != Field.Index.NO).SetIntValue((int)parsedVal);
                                break;
                            case "FLOAT":
                                if (!TryConvert<float>(x.Value, out parsedVal))
                                    break;
                                field = new NumericField(x.Key, Field.Store.YES, lucenePolicy != Field.Index.NO).SetFloatValue((float)parsedVal);
                                break;
                            case "DOUBLE":
                                if (!TryConvert<double>(x.Value, out parsedVal))
                                    break;
                                field = new NumericField(x.Key, Field.Store.YES, lucenePolicy != Field.Index.NO).SetDoubleValue((double)parsedVal);
                                break;
                            case "LONG":
                                if (!TryConvert<long>(x.Value, out parsedVal))
                                    break;
                                field = new NumericField(x.Key, Field.Store.YES, lucenePolicy != Field.Index.NO).SetLongValue((long)parsedVal);
                                break;
                            case "DATE":
                            case "DATETIME":
                                {
                                    if (!TryConvert<DateTime>(x.Value, out parsedVal))
                                        break;

                                    DateTime date = (DateTime)parsedVal;
                                    string dateAsString = DateTools.DateToString(date, DateTools.Resolution.MILLISECOND);
                                    //field = new NumericField(x.Key, Field.Store.YES, lucenePolicy != Field.Index.NO).SetLongValue(long.Parse(dateAsString));
                                    field =
                                    new Field(x.Key,
                                        dateAsString,
                                        Field.Store.YES,
                                        lucenePolicy,
                                        lucenePolicy == Field.Index.NO ? Field.TermVector.NO : Field.TermVector.YES
                                    );

                                    break;
                                }
                            case "DATE.YEAR":
                                {
                                    if (!TryConvert<DateTime>(x.Value, out parsedVal))
                                        break;

                                    DateTime date = (DateTime)parsedVal;
                                    string dateAsString = DateTools.DateToString(date, DateTools.Resolution.YEAR);
                                    //field = new NumericField(x.Key, Field.Store.YES, lucenePolicy != Field.Index.NO).SetIntValue(int.Parse(dateAsString));
                                    field =
                                    new Field(x.Key,
                                        dateAsString,
                                        Field.Store.YES,
                                        lucenePolicy,
                                        lucenePolicy == Field.Index.NO ? Field.TermVector.NO : Field.TermVector.YES
                                    );

                                    break;
                                }
                            case "DATE.MONTH":
                                {
                                    if (!TryConvert<DateTime>(x.Value, out parsedVal))
                                        break;

                                    DateTime date = (DateTime)parsedVal;
                                    string dateAsString = DateTools.DateToString(date, DateTools.Resolution.MONTH);
                                    //field = new NumericField(x.Key, Field.Store.YES, lucenePolicy != Field.Index.NO).SetIntValue(int.Parse(dateAsString));
                                    field =
                                    new Field(x.Key,
                                        dateAsString,
                                        Field.Store.YES,
                                        lucenePolicy,
                                        lucenePolicy == Field.Index.NO ? Field.TermVector.NO : Field.TermVector.YES
                                    );

                                    break;
                                }
                            case "DATE.DAY":
                                {
                                    if (!TryConvert<DateTime>(x.Value, out parsedVal))
                                        break;

                                    DateTime date = (DateTime)parsedVal;
                                    string dateAsString = DateTools.DateToString(date, DateTools.Resolution.DAY);
                                    //field = new NumericField(x.Key, Field.Store.YES, lucenePolicy != Field.Index.NO).SetIntValue(int.Parse(dateAsString));
                                    field =
                                    new Field(x.Key,
                                        dateAsString,
                                        Field.Store.YES,
                                        lucenePolicy,
                                        lucenePolicy == Field.Index.NO ? Field.TermVector.NO : Field.TermVector.YES
                                    );
                                    break;
                                }
                            case "DATE.HOUR":
                                {
                                    if (!TryConvert<DateTime>(x.Value, out parsedVal))
                                        break;

                                    DateTime date = (DateTime)parsedVal;
                                    string dateAsString = DateTools.DateToString(date, DateTools.Resolution.HOUR);
                                    //field = new NumericField(x.Key, Field.Store.YES, lucenePolicy != Field.Index.NO).SetIntValue(int.Parse(dateAsString));
                                    field =
                                    new Field(x.Key,
                                        dateAsString,
                                        Field.Store.YES,
                                        lucenePolicy,
                                        lucenePolicy == Field.Index.NO ? Field.TermVector.NO : Field.TermVector.YES
                                    );
                                    break;
                                }
                            case "DATE.MINUTE":
                                {
                                    if (!TryConvert<DateTime>(x.Value, out parsedVal))
                                        break;

                                    DateTime date = (DateTime)parsedVal;
                                    string dateAsString = DateTools.DateToString(date, DateTools.Resolution.MINUTE);
                                    //field = new NumericField(x.Key, Field.Store.YES, lucenePolicy != Field.Index.NO).SetIntValue(int.Parse(dateAsString));
                                    field =
                                    new Field(x.Key,
                                        dateAsString,
                                        Field.Store.YES,
                                        lucenePolicy,
                                        lucenePolicy == Field.Index.NO ? Field.TermVector.NO : Field.TermVector.YES
                                    );
                                    break;
                                }
                            default:
                                field =
                                    new Field(x.Key,
                                        x.Value,
                                        Field.Store.YES,
                                        lucenePolicy,
                                        lucenePolicy == Field.Index.NO ? Field.TermVector.NO : Field.TermVector.YES
                                    );
                                break;
                        }

                        //if the parsed value is null, this means it couldn't parse and we should log this error
                        if (field == null)
                        {
                            OnIndexingError(new IndexingErrorEventArgs("Could not parse value: " + x.Value + "into the type: " + indexField.Type, nodeId, null));
                        }
                        else
                        {
                            d.Add(field);

                            if (indexField.EnableSorting)
                            {
                                d.Add(new Field(SortedFieldNamePrefix + x.Key,
                                        x.Value,
                                        Field.Store.YES,
                                        Field.Index.NOT_ANALYZED,
                                        Field.TermVector.NO
                                        ));
                            }
                        }

                    }
                }
            }

            AddSpecialFieldsToDocument(d, fields);

            var docArgs = new DocumentWritingEventArgs(nodeId, d, fields);
            OnDocumentWriting(docArgs);
            if (docArgs.Cancel)
                return;

            writer.AddDocument(d);

            writer.Commit(); //commit changes!

            OnNodeIndexed(new IndexedNodeEventArgs(nodeId));
        }



        /// <summary>
        /// Returns a dictionary of special key/value pairs to store in the lucene index which will be stored by:
        /// - Field.Store.YES
        /// - Field.Index.NOT_ANALYZED_NO_NORMS
        /// - Field.TermVector.NO
        /// </summary>
        /// <param name="allValuesForIndexing">
        /// The dictionary object containing all name/value pairs that are to be put into the index
        /// </param>
        /// <returns></returns>
        protected virtual Dictionary<string, string> GetSpecialFieldsToIndex(Dictionary<string, string> allValuesForIndexing)
        {
            return new Dictionary<string, string>() 
			{
				//we want to store the nodeId separately as it's the index
				{IndexNodeIdFieldName, allValuesForIndexing[IndexNodeIdFieldName]},
				//add the index type first
				{IndexTypeFieldName, allValuesForIndexing[IndexTypeFieldName]}
			};
        }


        /// <summary>
        /// Process all of the queue items. This checks if this machine is the Executive and if it's in a load balanced
        /// environments. If then acts accordingly: 
        ///     Not the executive = doesn't index, i
        ///     In async mode = use file watcher timer
        /// </summary>
        protected void SafelyProcessQueueItems()
        {
            //if this is not the master indexer, exit
            if (!ExecutiveIndex.IsExecutiveMachine)
                return;

            //if in async mode, then process the queue using the timer            
            if (RunAsync)
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
        /// Inheritors should be very carefully using this method, SafelyProcessQueueItems will ensure
        /// that the correct machine processes the items into the index. SafelyQueueItems calls this method
        /// if it confirms that this machine is the one to process the queue.
        /// </remarks>
        protected int ForceProcessQueueItems()
        {
            try
            {
                ReInitialize();
            }
            catch (IOException ex)
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

            if (!m_IsIndexing)
            {
                lock (m_IndexerLocker)
                {
                    //double check
                    if (!m_IsIndexing)
                    {
                        //set our volatile flag
                        m_IsIndexing = true;

                        IndexWriter writer = null;
                        IndexReader reader = null;

                        //track all of the nodes indexed
                        var indexedNodes = new List<IndexedNode>();

                        try
                        {

                            //iterate through all files to add or delete to the index and index the content
                            //and order by file name since the file name is named with DateTime.Now.Ticks
                            //also order by extension descending so that the 'del' is processed before the 'add'
                            foreach (var x in IndexQueueItemFolder.GetFiles()
                                .Where(x => x.Extension == ".del" || x.Extension == ".add")
                                .OrderBy(x => x.Name)
                                .ThenByDescending(x => x.Extension) //we need to order by extension descending so that .del items are always processed before .add items
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
                                        OnIndexingError(new IndexingErrorEventArgs("Error indexing queue items, failed to obtain exclusive reader lock", -1, null), true);
                                        return indexedNodes.Count;
                                    }
                                }
                                else if (x.Extension == ".add")
                                {
                                    if (GetExclusiveIndexWriter(ref writer, ref reader))
                                    {
                                        indexedNodes.Add(ProcessAddQueueItem(x, writer));
                                    }
                                    else
                                    {
                                        OnIndexingError(new IndexingErrorEventArgs("Error indexing queue items, failed to obtain exclusive writer lock", -1, null), true);
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
                            //set our volatile flag
                            m_IsIndexing = false;

                            CloseWriter(ref writer);
                            CloseReader(ref reader);
                        }

                        //if there are enough commits, the we'll run an optimization
                        if (CommitCount >= OptimizationCommitThreshold)
                        {
                            OptimizeIndex();
                            CommitCount = 0; //reset the counter
                        }

                        return indexedNodes.Count;
                    }
                }
            }

            //if we get to this point, it means that another thead was beaten to the indexing operation so this thread will skip
            //this occurence.
            OnIndexingError(new IndexingErrorEventArgs("Cannot index queue items, another indexing operation is currently in progress", -1, null));
            return 0;


        }



        /// <summary>
        /// Saves a file indicating that the executive indexer should remove the from the index those that match
        /// the term saved in this file.
        /// This will save a file prefixed with the current machine name with an extension of .del
        /// </summary>
        /// <param name="term"></param>
        protected void SaveDeleteIndexQueueItem(KeyValuePair<string, string> term)
        {
            try
            {
                ReInitialize();
            }
            catch (IOException ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("Cannot save index queue item for deletion, an error occurred verifying queue folder", -1, ex));
                return;
            }

            var terms = new Dictionary<string, string>();
            terms.Add(term.Key, term.Value);
            var fileName = DateTime.Now.Ticks + "-" + Environment.MachineName;
            FileInfo fi = new FileInfo(Path.Combine(IndexQueueItemFolder.FullName, fileName + ".del"));

            //ok, everything is ready to go, but we'll conver the dictionary to a CData wrapped serialized version
            terms.SaveToDisk(fi);

        }

        /// <summary>
        /// Writes the information for the fields to a file names with the computer's name that is running the index and
        /// a GUID value. The indexer will then index the values stored in the files in another thread so that processing may continue.
        /// This will save a file prefixed with the current machine name with an extension of .add
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="nodeId">The node id.</param>
        /// <param name="type">The type.</param>
        protected void SaveAddIndexQueueItem(Dictionary<string, string> fields, int nodeId, string type)
        {
            try
            {
                ReInitialize();
            }
            catch (IOException ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("Cannot save index queue item, an error occurred verifying queue folder", nodeId, ex));
                return;
            }

            //ensure the special fields are added to the dictionary to be saved to file
            if (!fields.ContainsKey(IndexNodeIdFieldName))
                fields.Add(IndexNodeIdFieldName, nodeId.ToString());
            if (!fields.ContainsKey(IndexTypeFieldName))
                fields.Add(IndexTypeFieldName, type.ToString());

            var fileName = DateTime.Now.Ticks + "-" + Environment.MachineName + "-" + nodeId.ToString();
            FileInfo fi = new FileInfo(Path.Combine(IndexQueueItemFolder.FullName, fileName + ".add"));

            //ok, everything is ready to go, but we'll conver the dictionary to a CData wrapped serialized version
            fields.SaveToDisk(fi);
        }

        #endregion

        #region Private

        /// <summary>
        /// Tries to parse a type using the Type's type converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="parsedVal"></param>
        /// <returns></returns>
        private bool TryConvert<T>(string val, out object parsedVal)
            where T : struct
        {
            try
            {
                var t = typeof(T);
                TypeConverter tc = TypeDescriptor.GetConverter(t);
                parsedVal = (T)tc.ConvertFrom(val);
                return true;
            }
            catch (NotSupportedException)
            {
                parsedVal = null;
                return false;
            }

        }

        /// <summary>
        /// Adds 'special' fields to the Lucene index for use internally.
        /// By default this will add the __IndexType & __NodeId fields to the Lucene Index both specified by:
        /// - Field.Store.YES
        /// - Field.Index.NOT_ANALYZED_NO_NORMS
        /// - Field.TermVector.NO
        /// </summary>
        /// <param name="d"></param>
        private void AddSpecialFieldsToDocument(Document d, Dictionary<string, string> fields)
        {
            var specialFields = GetSpecialFieldsToIndex(fields);

            foreach (var s in specialFields)
            {
                //TODO: we're going to lower case the special fields, the Standard analyzer query parser always lower cases, so 
                //we need to do that... there might be a nicer way ?
                d.Add(new Field(s.Key, s.Value.ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO));
            }
        }

        /// <summary>
        /// This makes sure that the folders exist, that the executive indexer is setup and that the index is optimized.
        /// This is called at app startup when the providers are initialized but called again if folder are missing during a
        /// an indexing operation.
        /// </summary>
        private void ReInitialize()
        {

            //ensure all of the folders are created at startup   
            VerifyFolder(IndexSets.Instance.Sets[IndexSetName].IndexDirectory);
            VerifyFolder(LuceneIndexFolder);
            VerifyFolder(IndexQueueItemFolder);


            if (ExecutiveIndex == null)
            {
                ExecutiveIndex = new IndexerExecutive(IndexSets.Instance.Sets[IndexSetName].IndexDirectory);
            }

            if (!ExecutiveIndex.IsInitialized())
            {
                ExecutiveIndex.Initialize();

                //log some info if executive indexer
                if (ExecutiveIndex.IsExecutiveMachine)
                {
                    OnIndexerExecutiveAssigned(new IndexerExecutiveAssignedEventArgs(ExecutiveIndex.ExecutiveIndexerMachineName, ExecutiveIndex.ServerCount));
                }
            }
        }

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

            m_FileWatcher = new System.Timers.Timer(new TimeSpan(0, 0, IndexSecondsInterval).TotalMilliseconds);
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
        void FileWatcher_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
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
        /// <param name="reader"></param>
        /// <returns></returns>
        private bool GetExclusiveIndexWriter(ref IndexWriter writer, ref IndexReader reader)
        {
            //if the writer is already created, then we're ok
            if (writer != null)
                return true;

            //checks for locks and closes the reader if one is found
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

           

            writer = new IndexWriter(new SimpleFSDirectory(LuceneIndexFolder), IndexingAnalyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
            return true;
        }

        /// <summary>
        /// Checks the reader passed in to see if it is active, if not, checks if the index is locked. If it is locked, 
        /// returns checks if the writer is not null and tries to close it. if it's still locked returns null, otherwise
        /// creates a new reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="writer"></param>
        /// <returns>
        /// This also ensures that the reader is up to date, and if it is not, it re-opens the reader.
        /// </returns>
        private bool GetExclusiveIndexReader(ref IndexReader reader, ref IndexWriter writer)
        {
            //checks for locks and closes the writer if one is found
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

            if (reader != null)
            {
                //Turns out that each time we need to process one of these items, we'll need to refresh the reader since it won't be up
                //to date if the .add files are processed
                switch (reader.GetReaderStatus())
                {
                    case ReaderStatus.Current:
                        //if it's current, then we're ok
                        return true;
                    case ReaderStatus.NotCurrent:
                        //this will generally not be current each time an .add is processed and there's more deletes after the fact, we'll need to re-open

                        //yes, this is actually the way the Lucene wants you to work...
                        //normally, i would have thought just calling Reopen() on the underlying reader would suffice... but it doesn't.
                        //here's references: 
                        // http://stackoverflow.com/questions/1323779/lucene-indexreader-reopen-doesnt-seem-to-work-correctly
                        // http://gist.github.com/173978 
                        var oldReader = reader;
                        var newReader = oldReader.Reopen(false);
                        if (newReader != oldReader)
                        {
                            oldReader.Close();
                            reader = newReader;
                        }
                        //now that the reader is re-opened, we're good
                        return true;
                    case ReaderStatus.Closed:
                        //if it's closed, then we'll allow it to be opened below...
                        break;
                    default:
                        break;
                }
            }           
     
            //if we've made it this far, open a reader
            reader = IndexReader.Open(new SimpleFSDirectory(LuceneIndexFolder), false);
            return true;
            
        }

        /// <summary>
        /// Reads the FileInfo passed in into a dictionary object and deletes it from the index
        /// </summary>
        /// <param name="x"></param>
        /// <param name="ir"></param>
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

            CommitCount++;
        }

        /// <summary>
        /// Reads the FileInfo passed in into a dictionary object and adds it to the index
        /// </summary>
        /// <param name="x"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        private IndexedNode ProcessAddQueueItem(FileInfo x, IndexWriter writer)
        {
            //get the dictionary object from the file data
            SerializableDictionary<string, string> sd = new SerializableDictionary<string, string>();
            sd.ReadFromDisk(x);

            //get the node id
            int nodeId = int.Parse(sd[IndexNodeIdFieldName]);

            //now, add the index with our dictionary object
            AddDocument(sd, writer, nodeId, sd[IndexTypeFieldName]);

            //remove the file
            x.Delete();

            CommitCount++;

            return new IndexedNode() { NodeId = nodeId, Type = sd[IndexTypeFieldName] };
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
        /// Creates the folder if it does not exist.
        /// </summary>
        /// <param name="folder"></param>
        private void VerifyFolder(DirectoryInfo folder)
        {
            if (!System.IO.Directory.Exists(folder.FullName))
            {
                lock (m_FolderLocker)
                {
                    if (!System.IO.Directory.Exists(folder.FullName))
                    {
                        folder.Create();
                        folder.Refresh();
                    }
                }
            }

        }



        #endregion

        #region IDisposable Members

        protected bool _disposed;

        /// <summary>
        /// Checks the disposal state of the objects
        /// </summary>
        protected void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("LuceneExamine.BaseLuceneExamineIndexer");
        }

        /// <summary>
        /// When the object is disposed, all data should be written
        /// </summary>
        public void Dispose()
        {
            this.CheckDisposed();
            this.Dispose(true);
            GC.SuppressFinalize(this);
            this._disposed = true;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            this.CheckDisposed();
            if (disposing)
                this.m_FileWatcher.Dispose();
        }

        #endregion
    }
}
