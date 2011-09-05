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
using Lucene.Net.QueryParsers;
using Lucene.Net.Store;
using Examine.LuceneEngine.Config;
using Lucene.Net.Util;
using System.ComponentModel;
using System.Xml;

namespace Examine.LuceneEngine.Providers
{
    ///<summary>
    /// Abstract object containing all of the logic used to use Lucene as an indexer
    ///</summary>
    public abstract class LuceneIndexer : BaseIndexProvider, IDisposable
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        protected LuceneIndexer()
        {
            FileWatcher_ElapsedEventHandler = new System.Timers.ElapsedEventHandler(FileWatcher_Elapsed);
            IndexSecondsInterval = 5;
            OptimizationCommitThreshold = 100;
            AutomaticallyOptimize = true;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="workingFolder"></param>
        /// <param name="analyzer"></param>
        protected LuceneIndexer(IIndexCriteria indexerData, DirectoryInfo workingFolder, Analyzer analyzer, bool async)
            : base(indexerData)
        {
            FileWatcher_ElapsedEventHandler = new System.Timers.ElapsedEventHandler(FileWatcher_Elapsed);

            //set up our folders based on the index path
            WorkingFolder = workingFolder;
            LuceneIndexFolder = new DirectoryInfo(Path.Combine(workingFolder.FullName, "Index"));
            IndexQueueItemFolder = new DirectoryInfo(Path.Combine(workingFolder.FullName, "Queue"));

            IndexingAnalyzer = analyzer;

            //create our internal searcher, this is useful for inheritors to be able to search their own indexes inside of their indexer
            InternalSearcher = new LuceneSearcher(WorkingFolder, IndexingAnalyzer);

            IndexSecondsInterval = 5;
            OptimizationCommitThreshold = 100;
            RunAsync = async;
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

            if (config["autoOptimizeCommitThreshold"] == null)
            {
                OptimizationCommitThreshold = 100;
            }
            else
            {
                int autoCommitThreshold;
                if (int.TryParse(config["autoOptimizeCommitThreshold"], out autoCommitThreshold))
                {
                    OptimizationCommitThreshold = autoCommitThreshold;
                }
                else
                {
                    throw new ParseException("Could not parse autoCommitThreshold value into an integer");
                }
            }

            if (config["autoOptimize"] == null)
            {
                AutomaticallyOptimize = true;
            }
            else
            {
                bool autoOptimize;
                if (bool.TryParse(config["autoOptimize"], out autoOptimize))
                {
                    AutomaticallyOptimize = autoOptimize;
                }
                else
                {
                    throw new ParseException("Could not parse autoOptimize value into a boolean");
                }

            }

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
                        WorkingFolder = IndexSets.Instance.Sets[IndexSetName].IndexDirectory;
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
                    WorkingFolder = IndexSets.Instance.Sets[IndexSetName].IndexDirectory;
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
            InternalSearcher = new LuceneSearcher(WorkingFolder, IndexingAnalyzer);

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
        private readonly object _indexerLocker = new object();

        /// <summary>
        /// used to thread lock calls for creating and verifying folders
        /// </summary>
        private readonly object _folderLocker = new object();

        /// <summary>
        /// Used for double check locking during an index operation
        /// </summary>
        private bool _isIndexing = false;

        private System.Timers.Timer _fileWatcher = null;
        private System.Timers.ElapsedEventHandler FileWatcher_ElapsedEventHandler;

        /// <summary>
        /// We need an internal searcher used to search against our own index.
        /// This is used for finding all descendant nodes of a current node when deleting indexes.
        /// </summary>
        protected BaseSearchProvider InternalSearcher { get; private set; }

        #endregion

        #region Properties


        ///<summary>
        /// This will automatically optimize the index every 'AutomaticCommitThreshold' commits
        ///</summary>
        public bool AutomaticallyOptimize { get; protected set; }

        /// <summary>
        /// The number of commits to wait for before optimizing the index if AutomaticallyOptimize = true
        /// </summary>
        public int OptimizationCommitThreshold { get; internal set; }

        /// <summary>
        /// The analyzer to use when indexing content, by default, this is set to StandardAnalyzer
        /// </summary>
        public Analyzer IndexingAnalyzer { get; protected set; }

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
        public DirectoryInfo LuceneIndexFolder { get; private set; }

        /// <summary>
        /// The folder that stores the index queue files
        /// </summary>
        public DirectoryInfo IndexQueueItemFolder { get; private set; }

        /// <summary>
        /// The base folder that contains the queue and index folder and the indexer executive files
        /// </summary>
        public DirectoryInfo WorkingFolder { get; private set; }

        /// <summary>
        /// The Executive to determine if this is the master indexer
        /// </summary>
        protected IndexerExecutive ExecutiveIndex { get; set; }

        /// <summary>
        /// The index set name which references an Examine <see cref="IndexSet"/>
        /// </summary>
        public string IndexSetName { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when [index optimizing].
        /// </summary>
        public event EventHandler IndexOptimizing;

        ///<summary>
        /// Occurs when the index is finished optmizing
        ///</summary>
        public event EventHandler IndexOptimized;

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
                _isIndexing = false;
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

        protected virtual void OnIndexOptimized(EventArgs e)
        {
            if (IndexOptimized != null)
                IndexOptimized(this, e);
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

            //check if the index doesn't exist, and if so, create it and reindex everything, this will obviously index this
            //particular node
            if (!IndexExists())
            {
                RebuildIndex();
                return;
            }

            var buffer = new List<Dictionary<string, string>>();

            foreach (XElement node in nodes)
            {
                if (ValidateDocument(node))
                {
                    //save the index item to a queue file
                    var fields = GetDataToIndex(node, type);
                    BufferAddIndexQueueItem(fields, int.Parse((string)node.Attribute("id")), type, buffer);
                }
                else
                {
                    OnIgnoringNode(new IndexingNodeDataEventArgs(node, int.Parse(node.Attribute("id").Value), null, type));
                }

            }

            //now we need to save the buffer to disk
            SaveBufferAddIndexQueueItem(buffer);

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
                indexSet.IndexAttributeFields.Cast<IIndexField>().ToArray(),
                indexSet.IndexUserFields.Cast<IIndexField>().ToArray(),
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
        public virtual bool IndexExists()
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
            AddNodesToIndex(new XElement[] { node }, type);
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

            if (!_isIndexing)
            {
                lock (_indexerLocker)
                {
                    //double check
                    if (!_isIndexing)
                    {

                        //set our volatile flag
                        _isIndexing = true;

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

                            //wait for optimization to complete (true)
                            writer.Optimize(true);

                            OnIndexOptimized(new EventArgs());
                        }
                        catch (Exception ex)
                        {
                            OnIndexingError(new IndexingErrorEventArgs("Error optimizing Lucene index", -1, ex));
                        }
                        finally
                        {
                            //set our volatile flag
                            _isIndexing = false;

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
            var values = new Dictionary<string, string>();

            int nodeId = int.Parse(node.Attribute("id").Value);

            // Get all user data that we want to index and store into a dictionary 
            foreach (var field in IndexerData.UserFields)
            {
                // Get the value of the data                
                string value = node.SelectExamineDataValue(field.Name);

                //raise the event and assign the value to the returned data from the event
                var indexingFieldDataArgs = new IndexingFieldDataEventArgs(node, field.Name, value, false, nodeId);
                OnGatheringFieldData(indexingFieldDataArgs);
                value = indexingFieldDataArgs.FieldValue;

                //don't add if the value is empty/null
                if (!string.IsNullOrEmpty(value))
                {
                    if (!string.IsNullOrEmpty(value))
                        values.Add(field.Name, value);
                }
            }

            // Add umbraco node properties 
            foreach (var field in IndexerData.StandardFields)
            {
                string val = node.SelectExaminePropertyValue(field.Name);
                var args = new IndexingFieldDataEventArgs(node, field.Name, val, true, nodeId);
                OnGatheringFieldData(args);
                val = args.FieldValue;

                //don't add if the value is empty/null                
                if (!string.IsNullOrEmpty(val))
                {
                    values.Add(field.Name, val);
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
        /// <remarks>
        /// This will normalize (lowercase) all text before it goes in to the index.
        /// </remarks>
        protected virtual void AddDocument(Dictionary<string, string> fields, IndexWriter writer, int nodeId, string type)
        {
            var args = new IndexingNodeEventArgs(nodeId, fields, type);
            OnNodeIndexing(args);
            if (args.Cancel)
                return;

            var d = new Document();

            //get all index set fields that are defined
            var indexSetFields = IndexerData.UserFields.ToList().Concat(IndexerData.StandardFields.ToList());

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
                        var indexField = indexedFields.First();
                        Fieldable field = null;
                        object parsedVal = null;
                        if (string.IsNullOrEmpty(indexField.Type)) indexField.Type = string.Empty;
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
        protected internal void SafelyProcessQueueItems()
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

            if (!_isIndexing)
            {
                lock (_indexerLocker)
                {
                    //double check
                    if (!_isIndexing)
                    {
                        //set our volatile flag
                        _isIndexing = true;

                        IndexWriter writer = null;
                        IndexReader reader = null;

                        //track all of the nodes indexed
                        var indexedNodes = new List<IndexedNode>();

                        try
                        {
                            var batchDirs = IndexQueueItemFolder.GetDirectories()
                                .Where(x =>
                                           {
                                               //only return directories named as integers
                                               int i;
                                               return int.TryParse(x.Name, out i);
                                           })
                                .OrderByDescending(x => x.Name);
                            foreach (var batch in batchDirs)
                            {
                                batch.Refresh();
                                if (!batch.Exists)
                                {
                                    continue;
                                }
                                //iterate through all files to add or delete to the index and index the content
                                //and order by file name since the file name is named with DateTime.Now.Ticks
                                //also order by extension descending so that the 'del' is processed before the 'add'
                                foreach (var x in batch.GetFiles()
                                    .Where(x => x.Extension == ".del" || x.Extension == ".add")
                                    .OrderBy(x => x.Name)
                                    .ThenByDescending(x => x.Extension)) //we need to order by extension descending so that .del items are always processed before .add items
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
                                            try
                                            {
                                                //check if it's a buffered file... we'll base this on the file name
                                                if (x.Name.EndsWith("-buffered.add"))
                                                {
                                                    indexedNodes.AddRange(ProcessBufferedAddQueueItem(x, writer));
                                                }
                                                else
                                                {
                                                    indexedNodes.Add(ProcessAddQueueItem(x, writer));    
                                                }
                                            }
                                            catch (InvalidOperationException ex)
                                            {
                                                if (ex.InnerException != null && ex.InnerException is XmlException)
                                                {
                                                    OnIndexingError(new IndexingErrorEventArgs("Error reading index queue file, the XML is not properly formatted or contains invalid characters", -1, ex.InnerException));

                                                    //this will happen if the XML in the file is invalid and so that we can continue processing, we'll rename this
                                                    //file to have an extension of .error but move it to the main queue item folder
                                                    x.CopyTo(Path.Combine(IndexQueueItemFolder.FullName,
                                                                          string.Concat(x.Name.Substring(0, x.Name.Length - x.Extension.Length),
                                                                                        ".error")));
                                                    x.Delete();
                                                }
                                                else
                                                {
                                                    throw ex;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            OnIndexingError(new IndexingErrorEventArgs("Error indexing queue items, failed to obtain exclusive writer lock", -1, null), true);
                                            return indexedNodes.Count;
                                        }
                                    }
                                    //cleanup the batch folder
                                    if (batch.Name != "1" && !batch.GetFiles().Any())
                                    {
                                        batch.Delete();
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
                            _isIndexing = false;

                            CloseWriter(ref writer);
                            CloseReader(ref reader);
                        }

                        //if there are enough commits, the we'll run an optimization
                        if (AutomaticallyOptimize && CommitCount >= OptimizationCommitThreshold)
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
            var batchDir = GetQueueBatchFolder();
            var fileName = DateTime.Now.Ticks + "-" + Environment.MachineName;
            var fi = new FileInfo(Path.Combine(batchDir.FullName, fileName + ".del"));

            //ok, everything is ready to go, but we'll conver the dictionary to a CData wrapped serialized version
            terms.SaveToDisk(fi);

        }

        /// <summary>
        /// Used for re-indexing many nodes at once, this updates the fields object and appends it to the buffered list of items which
        /// will then get written to file in one bulk file.
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="nodeId"></param>
        /// <param name="type"></param>
        /// <param name="buffer"></param>
        protected void BufferAddIndexQueueItem(Dictionary<string, string> fields, int nodeId, string type, List<Dictionary<string, string>> buffer)
        {
            //ensure the special fields are added to the dictionary to be saved to file
            EnsureSpecialFields(fields, nodeId, type);
            
            //ok, everything is ready to go, add it to the buffer
            buffer.Add(fields);
        }

        /// <summary>
        /// Saves the buffered items to disk
        /// </summary>
        /// <param name="buffer"></param>
        protected void SaveBufferAddIndexQueueItem(List<Dictionary<string, string>> buffer)
        {
            var fileName = DateTime.Now.Ticks + "-" + Environment.MachineName + "-buffered";
            var batchDir = GetQueueBatchFolder();
            var fi = new FileInfo(Path.Combine(batchDir.FullName, fileName + ".add"));
            buffer.SaveToDisk(fi);
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
            EnsureSpecialFields(fields, nodeId, type);

            var fileName = DateTime.Now.Ticks + "-" + Environment.MachineName + "-" + nodeId.ToString();

            var batchDir = GetQueueBatchFolder();
            var fi = new FileInfo(Path.Combine(batchDir.FullName, fileName + ".add"));

            //ok, everything is ready to go, but we'll conver the dictionary to a CData wrapped serialized version
            fields.SaveToDisk(fi);
        }

        #endregion

        #region Private

        private void EnsureSpecialFields(Dictionary<string, string> fields, int nodeId, string type)
        {
            //ensure the special fields are added to the dictionary to be saved to file
            if (!fields.ContainsKey(IndexNodeIdFieldName))
                fields.Add(IndexNodeIdFieldName, nodeId.ToString());
            if (!fields.ContainsKey(IndexTypeFieldName))
                fields.Add(IndexTypeFieldName, type.ToString());
        }

        /// <summary>
        /// We put the queue files into batches of 500 seperated into folders. 
        /// This is to not fill up one folder with thousands of files for peformance reasons and windows file locks.
        /// This method will return the next available batch folder based on the file count threshold.
        /// </summary>
        /// <returns></returns>
        private DirectoryInfo GetQueueBatchFolder()
        {
            //we need to create sub folders for the queue as to not fill up an individual folder with thousands of files
            const int maxFiles = 500;
            DirectoryInfo batchDir;
            var dirs = IndexQueueItemFolder.GetDirectories().OrderByDescending(x =>
                                    {
                                        int name;
                                        return int.TryParse(x.Name, out name) ? name : 0;
                                    });
            if (dirs.Any())
            {
                //set the current batch to be executed in the highest named batch dir
                batchDir = dirs.First();
                //ensure its not already full
                if (batchDir.GetFiles()
                    .Where(x => x.Extension == ".del" || x.Extension == ".add")
                    .Count() >= maxFiles)
                {
                    //create a new batch folder
                    int i;
                    if (int.TryParse(batchDir.Name, out i))
                    {
                        batchDir = IndexQueueItemFolder.CreateSubdirectory((i + 1).ToString());
                    }
                }
            }
            else
            {
                //create a batch folder
                batchDir = IndexQueueItemFolder.CreateSubdirectory("1");
            }
            return batchDir;
        }
       
        /// <summary>
        /// Tries to parse a type using the Type's type converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="parsedVal"></param>
        /// <returns></returns>
        private static bool TryConvert<T>(string val, out object parsedVal)
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
            VerifyFolder(WorkingFolder);
            VerifyFolder(LuceneIndexFolder);
            VerifyFolder(IndexQueueItemFolder);


            if (ExecutiveIndex == null)
            {
                ExecutiveIndex = new IndexerExecutive(WorkingFolder);
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
            if (_fileWatcher != null)
            {
                //if this is not the master indexer anymore... perhaps another server has taken over somehow...
                if (!ExecutiveIndex.IsExecutiveMachine)
                {
                    //stop the timer, remove event handlers and close
                    _fileWatcher.Stop();
                    _fileWatcher.Elapsed -= FileWatcher_ElapsedEventHandler;
                    _fileWatcher.Dispose();
                    _fileWatcher = null;
                }

                return;
            }

            _fileWatcher = new System.Timers.Timer(new TimeSpan(0, 0, IndexSecondsInterval).TotalMilliseconds);
            _fileWatcher.Elapsed += FileWatcher_ElapsedEventHandler;
            _fileWatcher.AutoReset = false;
            _fileWatcher.Start();
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
            _fileWatcher.Stop();
            var numProcessedItems = 0;
            do
            {
                numProcessedItems = ForceProcessQueueItems();
            } while (numProcessedItems > 0);

            //restart the timer.
            _fileWatcher.Start();

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
            var sd = new SerializableDictionary<string, string>();
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

        private IEnumerable<IndexedNode> ProcessBufferedAddQueueItem(FileInfo x, IndexWriter writer)
        {
            var result = new List<IndexedNode>();

            //get the dictionary object from the file data
            var items = new List<Dictionary<string, string>>();
            //the entire xml representing the items, we'll use this for faster file saving so we don't have to
            //serialize/deserialize each time
            XDocument xDoc;
            items.ReadFromDisk(x, out xDoc);
            foreach(var sd in items)
            {
                //get the node id
                var nodeId = int.Parse(sd[IndexNodeIdFieldName]);

                //now, add the index with our dictionary object
                AddDocument(sd, writer, nodeId, sd[IndexTypeFieldName]);

                //update the file and remove the xml chunk we've just indexed
                xDoc.Root.FirstNode.Remove();
                xDoc.Save(x.FullName);

                CommitCount++;

                result.Add(new IndexedNode() { NodeId = nodeId, Type = sd[IndexTypeFieldName] });
            }

            //remove the file
            x.Delete();

            return result;
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
            var sd = new SerializableDictionary<string, string>();
            sd.ReadFromDisk(x);

            //get the node id
            var nodeId = int.Parse(sd[IndexNodeIdFieldName]);

            //now, add the index with our dictionary object
            AddDocument(sd, writer, nodeId, sd[IndexTypeFieldName]);

            CommitCount++;
                
            //remove the file
            x.Delete();

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
                lock (_folderLocker)
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
                this._fileWatcher.Dispose();
        }

        #endregion
    }
}
