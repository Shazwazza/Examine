using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Examine;
using Examine.LuceneEngine.Faceting;
using Examine.Providers;
using Examine.Session;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Contrib.Management;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Store;
using Examine.LuceneEngine.Config;
using Lucene.Net.Util;
using System.ComponentModel;
using System.Xml;
using LuceneManager.Infrastructure;

namespace Examine.LuceneEngine.Providers
{
    ///<summary>
    /// Abstract object containing all of the logic used to use Lucene as an indexer
    ///</summary>
    public abstract class LuceneIndexer : BaseIndexProvider, IDisposable
    {
        #region Constructors

        /// <summary>
        /// Default constructor.        
        /// If using this constructor, DO call EnsureIndex(false) when the indexer is initialized.
        /// </summary>
        protected LuceneIndexer()
        {
            OptimizationCommitThreshold = 100;
            AutomaticallyOptimize = true;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="workingFolder"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>
		[SecuritySafeCritical]
        protected LuceneIndexer(IIndexCriteria indexerData, DirectoryInfo workingFolder, Analyzer analyzer, bool async)
            : base(indexerData)
        {
            //set up our folders based on the index path
            WorkingFolder = workingFolder;
            LuceneIndexFolder = new DirectoryInfo(Path.Combine(workingFolder.FullName, "Index"));

            IndexingAnalyzer = analyzer;

            //create our internal searcher, this is useful for inheritors to be able to search their own indexes inside of their indexer
            InternalSearcher = new LuceneSearcher(WorkingFolder, IndexingAnalyzer);

            //IndexSecondsInterval = 5;
            OptimizationCommitThreshold = 100;
            AutomaticallyOptimize = true;
            RunAsync = async;

            EnsureIndex(false);
        }

		[SecuritySafeCritical]
		protected LuceneIndexer(IIndexCriteria indexerData, Lucene.Net.Store.Directory luceneDirectory, Analyzer analyzer, bool async)
			: base(indexerData)
		{			
			WorkingFolder = null;
			LuceneIndexFolder = null;
			_directory = luceneDirectory;

			IndexingAnalyzer = analyzer;

			//create our internal searcher, this is useful for inheritors to be able to search their own indexes inside of their indexer
			InternalSearcher = new LuceneSearcher(luceneDirectory, IndexingAnalyzer);

			//IndexSecondsInterval = 5;
			OptimizationCommitThreshold = 100;
			AutomaticallyOptimize = true;
			RunAsync = async;

            EnsureIndex(false);
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
		[SecuritySafeCritical]
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
                    throw new FormatException("Could not parse autoCommitThreshold value into an integer");
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
                    throw new FormatException("Could not parse autoOptimize value into a boolean");
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

                        //now set the index folders
                        WorkingFolder = IndexSets.Instance.Sets[IndexSetName].IndexDirectory;
                        LuceneIndexFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));

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

                    //now set the index folders
                    WorkingFolder = IndexSets.Instance.Sets[IndexSetName].IndexDirectory;
                    LuceneIndexFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));
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

            CommitCount = 0;

            EnsureIndex(false);
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
        private volatile bool _isIndexing = false;

        //private System.Timers.Timer _fileWatcher = null;
        //private System.Timers.ElapsedEventHandler FileWatcher_ElapsedEventHandler;

        /// <summary>
        /// We need an internal searcher used to search against our own index.
        /// This is used for finding all descendant nodes of a current node when deleting indexes.
        /// </summary>
        protected BaseSearchProvider InternalSearcher { get; private set; }

        ///// <summary>
        ///// This is our threadsafe queue of items which can be read by our background worker to process the queue
        ///// </summary>
        //private readonly ConcurrentQueue<IndexOperation> _indexQueue = new ConcurrentQueue<IndexOperation>();

        ///// <summary>
        ///// The async task that runs during an async indexing operation
        ///// </summary>
        //private Task _asyncTask;

        ///// <summary>
        ///// Used to cancel the async operation
        ///// </summary>
        //private volatile bool _isCancelling = false;

        //private bool _hasIndex = false;

        #endregion

        #region Static Helpers

        /// <summary>
        /// Converts a DateTime to total number of milliseconds for storage in a numeric field
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static long DateTimeToTicks(DateTime t)
        {
            return t.Ticks;
        }

        /// <summary>
        /// Converts a DateTime to total number of seconds for storage in a numeric field
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static double DateTimeToSeconds(DateTime t)
        {
            return (t - DateTime.MinValue).TotalSeconds;
        }

        /// <summary>
        /// Converts a DateTime to total number of minutes for storage in a numeric field
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double DateTimeToMinutes(DateTime t)
        {
            return (t - DateTime.MinValue).TotalMinutes;
        }

        /// <summary>
        /// Converts a DateTime to total number of hours for storage in a numeric field
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double DateTimeToHours(DateTime t)
        {
            return (t - DateTime.MinValue).TotalHours;
        }

        /// <summary>
        /// Converts a DateTime to total number of days for storage in a numeric field
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double DateTimeToDays(DateTime t)
        {
            return (t - DateTime.MinValue).TotalDays;
        }

        /// <summary>
        /// Converts a number of milliseconds to a DateTime from DateTime.MinValue
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>
        public static DateTime DateTimeFromTicks(long ticks)
        {
            return new DateTime(ticks);
        }

        /// <summary>
        /// Converts a number of seconds to a DateTime from DateTime.MinValue
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static DateTime DateTimeFromSeconds(double seconds)
        {
            return DateTime.MinValue.AddSeconds(seconds);
        }

        /// <summary>
        /// Converts a number of minutes to a DateTime from DateTime.MinValue
        /// </summary>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public static DateTime DateTimeFromMinutes(double minutes)
        {
            return DateTime.MinValue.AddMinutes(minutes);
        }

        /// <summary>
        /// Converts a number of hours to a DateTime from DateTime.MinValue
        /// </summary>
        /// <param name="hours"></param>
        /// <returns></returns>
        public static DateTime DateTimeFromHours(double hours)
        {
            return DateTime.MinValue.AddHours(hours);
        }

        /// <summary>
        /// Converts a number of days to a DateTime from DateTime.MinValue
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        public static DateTime DateTimeFromDays(double days)
        {
            return DateTime.MinValue.AddDays(days);
        }


        #endregion

        #region Properties


        ///<summary>
        /// This will automatically optimize the index every 'AutomaticCommitThreshold' commits
        ///</summary>
        [Obsolete("No longer used. Background thread handles optimization")]
        public bool AutomaticallyOptimize { get; protected set; }

        /// <summary>
        /// The number of commits to wait for before optimizing the index if AutomaticallyOptimize = true
        /// </summary>
        [Obsolete("No longer used. Background thread handles optimization")]
        public int OptimizationCommitThreshold { get; protected internal set; }

	    /// <summary>
	    /// The analyzer to use when indexing content, by default, this is set to StandardAnalyzer
	    /// </summary>
	    public Analyzer IndexingAnalyzer
	    {
			[SecuritySafeCritical]
		    get;
			[SecuritySafeCritical]
			protected set;
	    }

        /// <summary>
        /// Used to keep track of how many index commits have been performed.
        /// This is used to determine when index optimization needs to occur.
        /// </summary>
        public int CommitCount { get; protected internal set; }

        /// <summary>
        /// Indicates whether or this system will process the queue items asynchonously. Default is true.
        /// </summary>
        [Obsolete("Items are added synchroniously and commits and reopens are handled asynchroniously")]
        public bool RunAsync { get; protected internal set; }

        /// <summary>
        /// The folder that stores the Lucene Index files
        /// </summary>
        public DirectoryInfo LuceneIndexFolder { get; private set; }

        /// <summary>
        /// The base folder that contains the queue and index folder and the indexer executive files
        /// </summary>
        public DirectoryInfo WorkingFolder { get; private set; }

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
        /// Fires once an index operation is completed
        /// </summary>
        public event EventHandler IndexOperationComplete;

        /// <summary>
        /// Occurs when [document writing].
        /// </summary>
        public event EventHandler<DocumentWritingEventArgs> DocumentWriting;
        
        #endregion

        #region Event handlers
        
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

            //TODO: Maybe this exception shouldn't propagate to the user directly.
            if (!RunAsync)
            {
                var msg = "Indexing Error Occurred: " + e.Message;
                if (e.InnerException != null)
                    msg += ". ERROR: " + e.InnerException.Message;
                throw new Exception(msg, e.InnerException);
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

        protected virtual void OnIndexOperationComplete(EventArgs e)
        {
            if (IndexOperationComplete != null)
                IndexOperationComplete(this, e);
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
        /// Forces a particular XML node to be reindexed
        /// </summary>
        /// <param name="node">XML node to reindex</param>
        /// <param name="type">Type of index to use</param>
        public override void ReIndexNode(XElement node, string type)
        {   
            //now index the single node
            AddSingleNodeToIndex(node, type);
        }

        private volatile SearcherContext _searcherContext;
        public SearcherContext SearcherContext
        {
            get { return _searcherContext; }
        }

        /// <summary>
        /// Ensures that only one thread creates the searcher context
        /// </summary>
        private object _createLock = new object();

        /// <summary>
        /// Indicates that the index was created
        /// </summary>
        private bool _indexIsNew;

        /// <summary>
        /// Returns true if the index has just been created.
        /// On later requests it will return false
        /// </summary>
        /// <returns></returns>
        protected bool WasIndexCreated()
        {
            if (_indexIsNew)
            {
                _indexIsNew = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a brand new index, this will override any existing index with an empty one
        /// </summary>
		[SecuritySafeCritical]
        public void EnsureIndex(bool forceOverwrite)
        {
            if (_searcherContext == null)            
            {
                lock (_createLock)
                {                    
                    if (_searcherContext == null)
                    {
                        _indexIsNew = IndexExists();

                        SearcherContexts.Instance.RegisterContext(
                            _searcherContext = new SearcherContext(GetLuceneDirectory(), IndexingAnalyzer));
                        _searcherContext.Manager.Tracker = ExamineSession.TrackGeneration;
                    }
                }

            }            

            if (forceOverwrite)
            {
                _searcherContext.Manager.DeleteAll();
            }            
        }

        /// <summary>
        /// Rebuilds the entire index from scratch for all index types
        /// </summary>
        /// <remarks>This will completely delete the index and recreate it</remarks>
        public override void RebuildIndex()
        {
            EnsureIndex(true);

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
            EnqueueIndexOperation(new IndexOperation()
                {
                    Operation = IndexOperationType.Delete,
                    Item = new IndexItem(null, "", nodeId)
                });

            
            //SafelyProcessQueueItems();
        }

        /// <summary>
        /// Re-indexes all data for the index type specified
        /// </summary>
        /// <param name="type"></param>
        public override void IndexAll(string type)
        {
            //check if the index doesn't exist, and if so, create it and reindex everything
            if (WasIndexCreated())
            {
                RebuildIndex();
                return;
            }
            else
            {
                var op = new IndexOperation()
                    {
                        Operation = IndexOperationType.Delete,
                        Item = new IndexItem(null, type, string.Empty)
                    };
                EnqueueIndexOperation(op);                
            }

            //now do the indexing...
            PerformIndexAll(type);
        }

        #endregion

        /// <summary>
        /// This wil optimize the index for searching, this gets executed when this class instance is instantiated.
        /// </summary>
        /// <remarks>
        /// This can be an expensive operation and should only be called when there is no indexing activity
        /// </remarks>
		[SecuritySafeCritical]
        public void OptimizeIndex()
        {
            EnsureIndex(false);

            SearcherContext.Committer.OptimizeNow();

            //TODO: Hook into searchcontexts comitter thread to optimize


            //IndexWriter writer = null;
            //try
            //{
            //    if (!IndexExists())
            //        return;

            //    //check if the index is ready to be written to.
            //    if (!IndexReady())
            //    {
            //        OnIndexingError(new IndexingErrorEventArgs("Cannot optimize index, the index is currently locked", -1, null), true);
            //        return;
            //    }

            //    OnIndexOptimizing(new EventArgs());

            //    //open the writer for optization
            //    writer = GetIndexWriter();

            //    //wait for optimization to complete (true)
            //    writer.Optimize(true);

            //    OnIndexOptimized(new EventArgs());
            //}
            //catch (Exception ex)
            //{
            //    OnIndexingError(new IndexingErrorEventArgs("Error optimizing Lucene index", -1, ex));
            //}
            //finally
            //{
            //    CloseWriter(ref writer);
            //}

        }

        #region Protected

        /// <summary>
        /// This will add a number of nodes to the index
        /// </summary>        
        /// <param name="nodes"></param>
        /// <param name="type"></param>
        protected void AddNodesToIndex(IEnumerable<XElement> nodes, string type)
        {

            //check if the index doesn't exist, and if so, create it and reindex everything, this will obviously index this
            //particular node
            if (WasIndexCreated())
            {
                RebuildIndex();
                return;
            }
            
            foreach (XElement node in nodes)
            {
                EnqueueIndexOperation(new IndexOperation()
                {
                    Operation = IndexOperationType.Add,
                    Item = new IndexItem(node, type, (string)node.Attribute("id"))
                });
            }

            //run the indexer on all queued files
            //SafelyProcessQueueItems();
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
		[SecuritySafeCritical]
        protected bool IndexReady()
        {
            return (!IndexWriter.IsLocked(GetLuceneDirectory()));
        }

        /// <summary>
        /// Check if there is an index in the index folder
        /// </summary>
        /// <returns></returns>
		[SecuritySafeCritical]
        public override bool IndexExists()
        {
            return IndexReader.IndexExists(GetLuceneDirectory());
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
        /// Removes the specified term from the index
        /// </summary>
        /// <param name="indexTerm"></param>
        /// <param name="iw"></param>
        /// <param name="performCommit">Obsolete. Doesn't have any effect</param>
        /// <returns>Boolean if it successfully deleted the term, or there were on errors</returns>
		[SecuritySafeCritical]
        protected bool DeleteFromIndex(Term indexTerm, NrtManager iw, bool performCommit = true)
        {
            int nodeId = -1;
            if (indexTerm.Field() == "id")
                int.TryParse(indexTerm.Text(), out nodeId);

            try
            {
                //if the index doesn't exist, then no don't attempt to open it.
                if (!IndexExists())
                    return true;

                iw.DeleteDocuments(indexTerm);
                

                OnIndexDeleted(new DeleteIndexEventArgs(new KeyValuePair<string, string>(indexTerm.Field(), indexTerm.Text())));
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
            if (IndexerData.IncludeNodeTypes.Any())
                if (!IndexerData.IncludeNodeTypes.Contains(node.ExamineNodeTypeAlias()))
                    return false;

            //if this node type is part of our exclusion list, do not validate
            if (IndexerData.ExcludeNodeTypes.Any())
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

            var nodeId = int.Parse(node.Attribute("id").Value);

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
                    if (values.ContainsKey(field.Name))
                    {
                        OnDuplicateFieldWarning(nodeId, IndexSetName, field.Name);
                    }
                    else
                    {
                        values.Add(field.Name, val);    
                    }                    
                }

            }

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
                    if (values.ContainsKey(field.IndexName))
                    {
                        OnDuplicateFieldWarning(nodeId, IndexSetName, field.Name);
                    }
                    else
                    {
                        values.Add(field.IndexName, value);
                    }    
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
        [SecuritySafeCritical]
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
		[SecuritySafeCritical]
        protected virtual void AddDocument(Dictionary<string, string> fields, NrtManager writer, int nodeId, string type)
        {
            var args = new IndexingNodeEventArgs(nodeId, fields, type);
            OnNodeIndexing(args);
            if (args.Cancel)
                return;

            var d = new Document();

            //Add node id with payload term for fast retrieval in ReaderData              
            d.Add(new ExternalIdField(nodeId));


            //get all index set fields that are defined
            var indexSetFields = IndexerData.UserFields.Concat(IndexerData.StandardFields.ToList()).ToArray();

            //add all of our fields to the document index individually, don't include the special fields if they exists            
            var validFields = fields.Where(x => !x.Key.StartsWith(SpecialFieldPrefix)).ToArray();

            foreach (var x in validFields)
            {
                var ourPolicyType = GetPolicy(x.Key);
                var lucenePolicy = TranslateFieldIndexTypeToLuceneType(ourPolicyType);

                //copy local
                var x1 = x;
                var indexedFields = indexSetFields.Where(o => o.IndexName == x1.Key).ToArray();
                
                if (!indexedFields.Any())
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
                        OnDuplicateFieldWarning(nodeId, IndexSetName, x.Key);
                    }

                    //take the first one and continue!
                    var indexField = indexedFields.First();

                    Fieldable field = null;
                    Fieldable sortedField = null;
                    object parsedVal = null;
                    if (string.IsNullOrEmpty(indexField.Type)) indexField.Type = string.Empty;
                                                
                    switch (indexField.Type.ToUpper())
                    {
                        case "NUMBER":
                        case "INT":
                            if (!TryConvert<int>(x.Value, out parsedVal))
                                break;
                            field = new NumericField(x.Key, Field.Store.YES, lucenePolicy != Field.Index.NO).SetIntValue((int)parsedVal);
                            sortedField = new NumericField(SortedFieldNamePrefix + x.Key, Field.Store.NO, true).SetIntValue((int)parsedVal);
                            break;
                        case "FLOAT":
                            if (!TryConvert<float>(x.Value, out parsedVal))
                                break;
                            field = new NumericField(x.Key, Field.Store.YES, lucenePolicy != Field.Index.NO).SetFloatValue((float)parsedVal);
                            sortedField = new NumericField(SortedFieldNamePrefix + x.Key, Field.Store.NO, true).SetFloatValue((float)parsedVal);
                            break;
                        case "DOUBLE":
                            if (!TryConvert<double>(x.Value, out parsedVal))
                                break;
                            field = new NumericField(x.Key, Field.Store.YES, lucenePolicy != Field.Index.NO).SetDoubleValue((double)parsedVal);
                            sortedField = new NumericField(SortedFieldNamePrefix + x.Key, Field.Store.NO, true).SetDoubleValue((double)parsedVal);
                            break;
                        case "LONG":
                            if (!TryConvert<long>(x.Value, out parsedVal))
                                break;
                            field = new NumericField(x.Key, Field.Store.YES, lucenePolicy != Field.Index.NO).SetLongValue((long)parsedVal);
                            sortedField = new NumericField(SortedFieldNamePrefix + x.Key, Field.Store.NO, true).SetLongValue((long)parsedVal);
                            break;
                        case "DATE":
                        case "DATETIME":
                            {
                                SetDateTimeField(x.Key, x.Value, DateTools.Resolution.MILLISECOND, lucenePolicy, ref field, ref sortedField);
                                break;
                            }
                        case "DATE.YEAR":
                            {
                                SetDateTimeField(x.Key, x.Value, DateTools.Resolution.YEAR, lucenePolicy, ref field, ref sortedField);
                                break;
                            }
                        case "DATE.MONTH":
                            {
                                SetDateTimeField(x.Key, x.Value, DateTools.Resolution.MONTH, lucenePolicy, ref field, ref sortedField);
                                break;
                            }
                        case "DATE.DAY":
                            {
                                SetDateTimeField(x.Key, x.Value, DateTools.Resolution.DAY, lucenePolicy, ref field, ref sortedField);
                                break;
                            }
                        case "DATE.HOUR":
                            {
                                SetDateTimeField(x.Key, x.Value, DateTools.Resolution.HOUR, lucenePolicy, ref field, ref sortedField);
                                break;
                            }
                        case "DATE.MINUTE":
                            {
                                SetDateTimeField(x.Key, x.Value, DateTools.Resolution.MINUTE, lucenePolicy, ref field, ref sortedField);
                                break;
                            }
                            case "FACET": case "FACETPATH":
                                field = new Field(x.Key, x.Value, Field.Store.YES, Field.Index.NOT_ANALYZED);
                                break;
                        default:
                            field =
                                new Field(x.Key,
                                    x.Value,
                                    Field.Store.YES,
                                    lucenePolicy,
                                    lucenePolicy == Field.Index.NO ? Field.TermVector.NO : Field.TermVector.YES
                                );
                            sortedField = new Field(SortedFieldNamePrefix + x.Key,
                                                    x.Value,
                                                    Field.Store.NO, //we don't want to store the field because we're only using it to sort, not return data
                                                    Field.Index.NOT_ANALYZED,
                                                    Field.TermVector.NO
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

                        //add the special sorted field if sorting is enabled
                        if (indexField.EnableSorting)
                        {
                            d.Add(sortedField);
                        }
                    }
                }
            }

            AddSpecialFieldsToDocument(d, fields);

            var docArgs = new DocumentWritingEventArgs(nodeId, d, fields);
            OnDocumentWriting(docArgs);
            if (docArgs.Cancel)
                return;

            writer.UpdateDocument(new Term(IndexNodeIdFieldName, nodeId.ToString()), d);    

            OnNodeIndexed(new IndexedNodeEventArgs(nodeId));
        }

        [SecuritySafeCritical]
        private void SetDateTimeField(string fieldName, string valueToParse, DateTools.Resolution resolution, Field.Index lucenePolicy,
            ref Fieldable field, ref Fieldable sortedField)
        {
            object parsedVal;
            if (!TryConvert<DateTime>(valueToParse, out parsedVal))
                return;
            var date = (DateTime)parsedVal;
            string dateAsString = DateTools.DateToString(date, resolution);

            field =
                new Field(fieldName,
                          dateAsString,
                          Field.Store.YES,
                          lucenePolicy,
                          lucenePolicy == Field.Index.NO ? Field.TermVector.NO : Field.TermVector.YES
                    );

            sortedField =
                new Field(SortedFieldNamePrefix + fieldName,
                          dateAsString,
                          Field.Store.NO, //do not store, we're not going to return this value only use it for sorting
                          Field.Index.NOT_ANALYZED,
                          Field.TermVector.NO
                    );
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

        ///// <summary>
        ///// Process all of the queue items
        ///// </summary>
        //protected internal void SafelyProcessQueueItems()
        //{
            
        //    if (!RunAsync)
        //    {
        //        StartIndexing();
        //    }
        //    else
        //    {                
        //        if (!_isIndexing)
        //        {
        //            //don't run the worker if it's currently running since it will just pick up the rest of the queue during its normal operation                    
        //            lock (_indexerLocker)
        //            {
        //                if (!_isIndexing && (_asyncTask == null || _asyncTask.IsCompleted))
        //                {
        //                    //Debug.WriteLine("Examine: Launching task");
        //                    _asyncTask = Task.Factory.StartNew(StartIndexing, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        //                }
        //            }
        //        }
        //    }

        //}

        ///// <summary>
        ///// Processes the queue and checks if optimization needs to occur at the end
        ///// </summary>
        //void StartIndexing()
        //{
        //    if (!_isIndexing)
        //    {
        //        lock (_indexerLocker)
        //        {
        //            if (!_isIndexing)
        //            {
  
        //                _isIndexing = true;

        //                //keep processing until it is complete
        //                var numProcessedItems = 0;
        //                do
        //                {
        //                    numProcessedItems = ForceProcessQueueItems();
        //                } while (!_isCancelling && numProcessedItems > 0);

        //                //if there are enough commits, then we'll run an optimization
        //                if (CommitCount >= OptimizationCommitThreshold)
        //                {
        //                    OptimizeIndex();
        //                    CommitCount = 0; //reset the counter
        //                }

        //                //reset the flag
        //                _isIndexing = false;

        //                OnIndexOperationComplete(new EventArgs());
        //            }
        //        }
        //    }

        //}

        
        protected void ProcessIndexOperation(IndexOperation item)
        {
            switch (item.Operation)
            {
                case IndexOperationType.Add:

                    //NOTE: No need to delete here. UpdateDocument is used when it is added.

                    ////check if it is already in our index
                    //var idResult = inMemorySearcher.Search(inMemorySearcher.CreateSearchCriteria().Id(int.Parse(item.Item.Id)).Compile());
                    ////if one is found, then delete it from the main index before the fast index is merged in
                    //if (idResult.Any())
                    //{
                    //    //do the delete but no commit
                    //    ProcessDeleteQueueItem(item, realWriter, false);
                    //}

                    if (ValidateDocument(item.Item.DataToIndex))
                    {
                        var node = ProcessIndexQueueItem(item, SearcherContext.Manager);
                        OnNodesIndexed(new IndexedNodesEventArgs(IndexerData, new[] {node}));
                    }
                    else
                    {
                        OnIgnoringNode(new IndexingNodeDataEventArgs(item.Item.DataToIndex, int.Parse(item.Item.Id), null, item.Item.IndexType));
                    }
                    break;
                case IndexOperationType.Delete:
                    ProcessDeleteQueueItem(item, SearcherContext.Manager, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        protected void EnqueueIndexOperation(IndexOperation op)
        {
            ProcessIndexOperation(op);    
            //_indexQueue.Enqueue(op);
        }

        private Lucene.Net.Store.Directory _directory;

        /// <summary>
        /// Returns the Lucene Directory used to store the index
        /// </summary>
        /// <returns></returns>
		[SecuritySafeCritical]
        public virtual Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            if (_directory == null)
            {
                //ensure all of the folders are created at startup   
                VerifyFolder(WorkingFolder);
                VerifyFolder(LuceneIndexFolder);
                _directory = new SimpleFSDirectory(LuceneIndexFolder);
            }
            return _directory;
        }

        /// <summary>
        /// Returns an index writer for the current directory
        /// </summary>
        /// <returns></returns>
		[SecuritySafeCritical]
        
        public virtual NrtManager GetIndexWriter()
        {
            EnsureIndex(false);
            return SearcherContext.Manager;
        }

        


        #endregion

        #region Private

        private void EnsureSpecialFields(Dictionary<string, string> fields, string nodeId, string type)
        {
            //ensure the special fields are added to the dictionary to be saved to file
            if (!fields.ContainsKey(IndexNodeIdFieldName))
                fields.Add(IndexNodeIdFieldName, nodeId);
            if (!fields.ContainsKey(IndexTypeFieldName))
                fields.Add(IndexTypeFieldName, type);
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
		[SecuritySafeCritical]
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


        ///// <summary>
        ///// Creates a new in-memory index with a writer for it
        ///// </summary>
        ///// <returns></returns>
        //[SecuritySafeCritical]
        //private IndexWriter GetNewInMemoryWriter()
        //{
        //    return new IndexWriter(new Lucene.Net.Store.RAMDirectory(), IndexingAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
        //}

        /// <summary>
        /// Reads the FileInfo passed in into a dictionary object and deletes it from the index
        /// </summary>
        /// <param name="op"></param>
        /// <param name="iw"></param>
        /// <param name="performCommit"></param>
		[SecuritySafeCritical]
        private void ProcessDeleteQueueItem(IndexOperation op, NrtManager iw, bool performCommit = true)
        {
           
            //if the id is empty then remove the whole type
            if (string.IsNullOrEmpty(op.Item.Id))
            {
                DeleteFromIndex(new Term(IndexTypeFieldName, op.Item.IndexType), iw, performCommit);
            }
            else
            {
                DeleteFromIndex(new Term(IndexNodeIdFieldName, op.Item.Id), iw, performCommit);    
            }

            CommitCount++;
        }

		[SecuritySafeCritical]
        private IndexedNode ProcessIndexQueueItem(IndexOperation op, NrtManager writer)
        {
            //get the node id
            var nodeId = int.Parse(op.Item.Id);

            //now, add the index with our dictionary object
            var fields = GetDataToIndex(op.Item.DataToIndex, op.Item.IndexType);
            EnsureSpecialFields(fields, op.Item.Id, op.Item.IndexType);

            AddDocument(fields, writer, nodeId, op.Item.IndexType);

            CommitCount++;

            return new IndexedNode() {NodeId = nodeId, Type = op.Item.IndexType};
        }

        //[SecuritySafeCritical]
        //private void CloseWriter(ref IndexWriter writer)
        //{
        //    if (writer != null)
        //    {
        //        writer.Close();
        //        writer = null;
        //    }
        //}

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
            {
                //_isCancelling = true;
                //this._fileWatcher.Dispose();
            }
                
        }

        #endregion
    }
}
