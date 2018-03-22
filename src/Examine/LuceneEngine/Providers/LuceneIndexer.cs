using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Directories;
using Examine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;


namespace Examine.LuceneEngine.Providers
{
    ///<summary>
    /// Abstract object containing all of the logic used to use Lucene as an indexer
    ///</summary>
    public abstract class LuceneIndexer : BaseIndexProvider, IDisposable
    {
        #region Constructors

        /// <summary>
        /// Default constructor - used for defining indexes in config
        /// </summary>
        
        protected LuceneIndexer()
        {
            OptimizationCommitThreshold = 100;
            _disposer = new DisposableIndexer(this);
            _committer = new IndexCommiter(this);
            _internalSearcher = new Lazy<LuceneSearcher>(GetSearcher);
            WaitForIndexQueueOnShutdown = true;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="workingFolder"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>
        
        protected LuceneIndexer(IIndexCriteria indexerData, DirectoryInfo workingFolder, Analyzer analyzer, bool async)
            : base(indexerData)
        {
            _disposer = new DisposableIndexer(this);
            _committer = new IndexCommiter(this);

            //set up our folders based on the index path
            WorkingFolder = workingFolder;
            LuceneIndexFolder = new DirectoryInfo(Path.Combine(workingFolder.FullName, "Index"));

            IndexingAnalyzer = analyzer;

            //IndexSecondsInterval = 5;
            OptimizationCommitThreshold = 100;
            RunAsync = async;

            InitializeDirectory();
            _internalSearcher = new Lazy<LuceneSearcher>(GetSearcher);
            WaitForIndexQueueOnShutdown = true;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="luceneDirectory"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>
        
        protected LuceneIndexer(IIndexCriteria indexerData, Directory luceneDirectory, Analyzer analyzer, bool async)
            : base(indexerData)
        {
            _disposer = new DisposableIndexer(this);
            _committer = new IndexCommiter(this);

            WorkingFolder = null;
            LuceneIndexFolder = null;

            IndexingAnalyzer = analyzer;

            //IndexSecondsInterval = 5;
            OptimizationCommitThreshold = 100;
            RunAsync = async;

            _directory = luceneDirectory;
            _internalSearcher = new Lazy<LuceneSearcher>(GetSearcher);
            WaitForIndexQueueOnShutdown = true;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime - using NRT
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="writer"></param>
        /// <param name="async"></param>
        
        protected LuceneIndexer(IIndexCriteria indexerData, IndexWriter writer, bool async)
            : base(indexerData)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            _disposer = new DisposableIndexer(this);
            _committer = new IndexCommiter(this);

            _writer = writer;
            WorkingFolder = null;
            LuceneIndexFolder = null;

            IndexingAnalyzer = writer.Analyzer;

            //IndexSecondsInterval = 5;
            OptimizationCommitThreshold = 100;
            RunAsync = async;
            _internalSearcher = new Lazy<LuceneSearcher>(GetSearcher);
            WaitForIndexQueueOnShutdown = true;
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
        
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            if (config["directoryFactory"] != null)
            {
                //this should be a fully qualified type
                var factoryType = TypeHelper.FindType(config["directoryFactory"]);
                if (factoryType == null) throw new NullReferenceException("No directory type found for value: " + config["directoryFactory"]);
                DirectoryFactory = (IDirectoryFactory)Activator.CreateInstance(factoryType);
            }

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

            //Need to check if the index set or IndexerData is specified...

            if (config["indexSet"] == null && IndexerData == null)
            {
                //if we don't have either, then we'll try to set the index set by naming conventions
                var found = false;
                if (name.EndsWith("Indexer"))
                {
                    var setNameByConvension = name.Remove(name.LastIndexOf("Indexer")) + "IndexSet";
                    //check if we can assign the index set by naming convention
                    var set = IndexSets.Instance.Sets.Cast<IndexSet>().SingleOrDefault(x => x.SetName == setNameByConvension);

                    if (set != null)
                    {
                        //we've found an index set by naming conventions :)
                        IndexSetName = set.SetName;

                        var indexSet = IndexSets.Instance.Sets[IndexSetName];

                        //if tokens are declared in the path, then use them (i.e. {machinename} )
                        indexSet.ReplaceTokensInIndexPath();

                        //get the index criteria and ensure folder
                        IndexerData = GetIndexerData(indexSet);

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

                    var indexSet = IndexSets.Instance.Sets[IndexSetName];

                    //if tokens are declared in the path, then use them (i.e. {machinename} )
                    indexSet.ReplaceTokensInIndexPath();

                    //get the index criteria and ensure folder
                    IndexerData = GetIndexerData(indexSet);

                    //now set the index folders
                    WorkingFolder = IndexSets.Instance.Sets[IndexSetName].IndexDirectory;
                    LuceneIndexFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));
                }
            }

            InitializeDirectory();

            if (config["analyzer"] != null)
            {
                //this should be a fully qualified type
                var analyzerType = TypeHelper.FindType(config["analyzer"]);
                IndexingAnalyzer = (Analyzer)Activator.CreateInstance(analyzerType);
            }
            else
            {
                IndexingAnalyzer = new StandardAnalyzer(Version.LUCENE_29);
            }

            RunAsync = true;
            if (config["runAsync"] != null)
            {
                RunAsync = bool.Parse(config["runAsync"]);
            }

            CommitCount = 0;

        }

        #endregion

        #region Constants & Fields

        private volatile IndexWriter _writer;

        private int _activeWrites = 0;
        private int _activeAddsOrDeletes = 0;

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
        private readonly object _indexingLocker = new object();

        /// <summary>
        /// Used to aquire the index writer
        /// </summary>
        private readonly object _writerLocker = new object();

        /// <summary>
        /// used to thread lock calls for creating and verifying folders
        /// </summary>
        private readonly object _folderLocker = new object();

        /// <summary>
        /// Used for double check locking during an index operation
        /// </summary>
        private volatile bool _isIndexing = false;

        private readonly Lazy<LuceneSearcher> _internalSearcher;

        private bool? _exists;

        /// <summary>
        /// We need an internal searcher used to search against our own index.
        /// This is used for finding all descendant nodes of a current node when deleting indexes.
        /// </summary>       
        protected virtual BaseSearchProvider InternalSearcher
        {
            
            get { return _internalSearcher.Value; }
        }

        /// <summary>
        /// This is our threadsafe queue of items which can be read by our background worker to process the queue
        /// </summary>
        /// <remarks>
        /// Each item in the collection is a collection itself, this allows us to have lazy access to a collection as part of the queue if added in bulk
        /// </remarks>
        private readonly BlockingCollection<IEnumerable<IndexOperation>> _indexQueue = new BlockingCollection<IEnumerable<IndexOperation>>();

        /// <summary>
        /// The async task that runs during an async indexing operation
        /// </summary>
        private Task _asyncTask;

        /// <summary>
        /// Used to cancel the async operation
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private bool _hasIndex = false;

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

        /// <summary>
        /// this flag indicates if Examine should wait for the current index queue to be fully processed during appdomain shutdown
        /// </summary>
        /// <remarks>
        /// By default this is true but in some cases a user may wish to disable this since this can block an appdomain from shutting down
        /// within a reasonable time which can cause problems with overlapping appdomains.
        /// </remarks>
        public bool WaitForIndexQueueOnShutdown { get; set; }
        
        /// <summary>
        /// The number of commits to wait for before optimizing the index if AutomaticallyOptimize = true
        /// </summary>
        public int OptimizationCommitThreshold { get; protected internal set; }

        /// <summary>
        /// The analyzer to use when indexing content, by default, this is set to StandardAnalyzer
        /// </summary>
        public Analyzer IndexingAnalyzer
        {
            
            get;
            
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

        /// <summary>
        /// returns true if the indexer has been canceled (app is shutting down)
        /// </summary>
        protected bool IsCancellationRequested
        {
            get { return _cancellationTokenSource.IsCancellationRequested; }
        }

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

#if FULLDEBUG
            Trace.TraceError("Indexing Error Occurred: " + (e.InnerException == null ?  e.Message : e.Message + " -- " + e.InnerException));
#endif

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

        /// <summary>
        /// Creates a brand new index, this will override any existing index with an empty one
        /// </summary>
        
        public void EnsureIndex(bool forceOverwrite)
        {
            if (!forceOverwrite && _hasIndex) return;

            if (!IndexExists() || forceOverwrite)
            {
                //if we can't acquire the lock exit - this will happen if this method is called multiple times but we don't want this 
                // logic to actually execute multiple times
                if (Monitor.TryEnter(_writerLocker))
                {
                    try
                    {
                        var dir = GetLuceneDirectory();

                        //if there's no index, we need to create one
                        if (!IndexExists())
                        {
                            CreateNewIndex(dir);
                        }
                        else if (forceOverwrite)
                        {
                            Trace.WriteLine("Initializing new index");

                            if (_writer == null)
                            {
                                //This will happen if the writer hasn't been created/initialized yet which
                                // might occur if a rebuild is triggered before any indexing has been triggered.
                                //In this case we need to initialize a writer and continue as normal.
                                //Since we are already inside the writer lock and it is null, we are allowed to 
                                // make this call with out using GetIndexWriter() to do the initialization.
                                _writer = CreateIndexWriter();
                            }

                            //We're forcing an overwrite, 
                            // this means that we need to cancel all operations currently in place,
                            // clear the queue and delete all of the data in the index.

                            //cancel any operation currently in place
                            _cancellationTokenSource.Cancel();

                            try
                            {
                                //clear the queue
                                IEnumerable<IndexOperation> op;
                                while (_indexQueue.TryTake(out op))
                                {
                                }

                                //remove all of the index data
                                _writer.DeleteAll();
                                _writer.Commit();

                                //we're rebuilding so all old readers referencing this dir should be closed
                                OpenReaderTracker.Current.CloseStaleReaders(dir, TimeSpan.FromMinutes(1));
                            }
                            finally
                            {
                                _cancellationTokenSource = new CancellationTokenSource();
                            }
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_writerLocker);
                    }
                }
                else
                {
                    // we cannot acquire the lock, this is because the main writer is being created, or the index is being created currently
                    OnIndexingError(new IndexingErrorEventArgs("Could not acquire lock in EnsureIndex so cannot create new index", -1, null));
                }
            }
        }

        /// <summary>
        /// Used internally to create a brand new index, this is not thread safe
        /// </summary>
        
        private void CreateNewIndex(Directory dir)
        {
            IndexWriter writer = null;
            try
            {
                if (IndexWriter.IsLocked(dir))
                {
                    //unlock it!
                    IndexWriter.Unlock(dir);
                }
                //create the writer (this will overwrite old index files)
                writer = new IndexWriter(dir, IndexingAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("An error occurred creating the index", -1, ex));
                return;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
                _hasIndex = true;
            }
        }

        /// <summary>
        /// Rebuilds the entire index from scratch for all index types
        /// </summary>
        /// <remarks>This will completely delete the index and recreate it</remarks>
        public override void RebuildIndex()
        {
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                OnIndexingError(new IndexingErrorEventArgs("Cannot rebuild the index, indexing cancellation has been requested", -1, null));
                return;
            }

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
            Interlocked.Increment(ref _activeAddsOrDeletes);

            try
            {
                EnqueueIndexOperation(new IndexOperation()
                {
                    Operation = IndexOperationType.Delete,
                    Item = new IndexItem(null, "", nodeId)
                });
                SafelyProcessQueueItems();
            }
            finally
            {
                Interlocked.Decrement(ref _activeAddsOrDeletes);
            }
        }

        /// <summary>
        /// Re-indexes all data for the index type specified
        /// </summary>
        /// <param name="type"></param>
        public override void IndexAll(string type)
        {
            //remove this index type
            var op = new IndexOperation()
            {
                Operation = IndexOperationType.Delete,
                Item = new IndexItem(null, type, string.Empty)
            };
            EnqueueIndexOperation(op);

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
        
        public void OptimizeIndex()
        {
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                OnIndexingError(new IndexingErrorEventArgs("Cannot optimize index, index cancellation has been requested", -1, null), true);
                return;
            }

            try
            {
                if (!IndexExists())
                    return;

                //check if the index is ready to be written to.
                if (!IndexReady())
                {
                    OnIndexingError(new IndexingErrorEventArgs("Cannot optimize index, the index is currently locked", -1, null), true);
                    return;
                }

                OnIndexOptimizing(new EventArgs());

                //open the writer for optization
                var writer = GetIndexWriter();

                //wait for optimization to complete (true)
                writer.Optimize(true);

                OnIndexOptimized(new EventArgs());
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("Error optimizing Lucene index", -1, ex));
            }

        }

        #region Protected

        /// <summary>
        /// This will add a number of nodes to the index
        /// </summary>        
        /// <param name="nodes"></param>
        /// <param name="type"></param>
        protected void AddNodesToIndex(IEnumerable<XElement> nodes, string type)
        {

            //need to lock, we don't want to issue any node writing if there's an index rebuild occuring
            Monitor.Enter(_writerLocker);
            try
            {
                Interlocked.Increment(ref _activeAddsOrDeletes);

                try
                {
                    //enqueue the batch, this allows lazy enumeration of the items
                    // when the indexes starts to process
                    EnqueueIndexOperation(
                        nodes.Select(node => new IndexOperation(new IndexItem(node, type, (string)node.Attribute("id")), IndexOperationType.Add)));

                    //run the indexer on all queued files
                    SafelyProcessQueueItems();
                }
                finally
                {
                    Interlocked.Decrement(ref _activeAddsOrDeletes);
                }
            }
            finally
            {
                Monitor.Exit(_writerLocker);
            }
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
            return _writer != null || (!IndexWriter.IsLocked(GetLuceneDirectory()));
        }

        /// <summary>
        /// Check if there is an index in the index folder
        /// </summary>
        /// <returns></returns>
        
        public override bool IndexExists()
        {
            return _writer != null || IndexExistsImpl();
        }

        /// <summary>
        /// Check if the index is readable/healthy
        /// </summary>
        /// <returns></returns>
        
        internal bool IsReadable(out Exception ex)
        {
            if (_writer != null)
            {
                try
                {
                    using (_writer.GetReader())
                    {
                        ex = null;
                        return true;
                    }
                }
                catch (Exception e)
                {
                    ex = e;
                    return false;
                }
            }

            try
            {
                using (IndexReader.Open(GetLuceneDirectory(), true))
                {                    
                }
                ex = null;
                return true;
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }

        /// <summary>
        /// This will check one time if the index exists, we don't want to keep using IndexReader.IndexExists because that will literally go list
        /// every file in the index folder and we don't need any more IO ops
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// If the index does not exist, it will not store the value so subsequent calls to this will re-evaulate
        /// </remarks>
        
        private bool IndexExistsImpl()
        {
            //if it's been set and it's true, return true
            if (_exists.HasValue && _exists.Value) return true;

            //if it's not been set or it just doesn't exist, re-read the lucene files
            if (!_exists.HasValue || !_exists.Value)
            {
                _exists = IndexReader.IndexExists(GetLuceneDirectory());
            }

            return _exists.Value;
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
        /// <param name="performCommit"></param>
        /// <returns>Boolean if it successfully deleted the term, or there were on errors</returns>
        
        protected bool DeleteFromIndex(Term indexTerm, IndexWriter iw, bool performCommit = true)
        {
            int nodeId = -1;
            if (indexTerm.Field == "id")
                int.TryParse(indexTerm.Text, out nodeId);

            try
            {
                //if the index doesn't exist, then no don't attempt to open it.
                if (!IndexExists())
                    return true;

                iw.DeleteDocuments(indexTerm);

                if (performCommit)
                {
                    iw.Commit();
                }


                OnIndexDeleted(new DeleteIndexEventArgs(new KeyValuePair<string, string>(indexTerm.Field, indexTerm.Text)));
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
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!node.IsExamineElement())
                return values;
            
            //resolve all attributes now it is much faster to do this than to relookup all of the XML data
            //using Linq and the node.Attributes() methods re-gets all of them.
            var attributeValues = node.Attributes().ToDictionary(x => x.Name.LocalName, x => x.Value);

            var nodeId = int.Parse(attributeValues["id"]);

            // Add umbraco node properties 
            foreach (var field in IndexerData.StandardFields)
            {
                string val = node.SelectExaminePropertyValue(attributeValues, field.Name);
                if (val == null) continue;

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

            //resolve all element data now it is much faster to do this than to relookup all of the XML data
            //using Linq and the node.Elements() methods re-gets all of them.
            var elementValues = node.SelectExamineDataValues();
            
            // Get all user data that we want to index and store into a dictionary 
            foreach (var field in IndexerData.UserFields)
            {
                // Get the value of the data       
                string value;
                if (!elementValues.TryGetValue(field.Name, out value))
                    continue;

                //raise the event and assign the value to the returned data from the event
                var indexingFieldDataArgs = new IndexingFieldDataEventArgs(node, field.Name, value, false, nodeId);
                OnGatheringFieldData(indexingFieldDataArgs);
                value = indexingFieldDataArgs.FieldValue;

                //don't add if the value is empty/null
                if (!string.IsNullOrEmpty(value))
                {
                    if (values.ContainsKey(field.Name))
                    {
                        OnDuplicateFieldWarning(nodeId, IndexSetName, field.Name);
                    }
                    else
                    {
                        values.Add(field.Name, value);
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
            
            foreach (var field in fields)
            {
                //don't include the special fields if they exists     
                if (field.Key.StartsWith(SpecialFieldPrefix))
                    continue;

                var ourPolicyType = GetPolicy(field.Key);
                var lucenePolicy = TranslateFieldIndexTypeToLuceneType(ourPolicyType);

                IReadOnlyList<IIndexField> indexedFields;
                if (!CombinedIndexerDataFields.TryGetValue(field.Key, out indexedFields))
                {
                    indexedFields = new List<IIndexField>();
                }

                if (indexedFields.Count == 0)
                {
                    //TODO: Decide if we should support non-strings in here too
                    d.Add(
                    new Field(field.Key,
                        field.Value,
                        Field.Store.YES,
                        lucenePolicy,
                              Equals(lucenePolicy, Field.Index.NO) ? Field.TermVector.NO : Field.TermVector.YES));
                }

                else
                {
                    //checks if there's duplicates fields, if not check if the field needs to be sortable...
                    if (indexedFields.Count > 1)
                    {
                        //we wont error if there are two fields which match, we'll just log an error and ignore the 2nd field                        
                        OnDuplicateFieldWarning(nodeId, IndexSetName, field.Key);
                    }

                    //take the first one and continue!
                    var indexField = indexedFields[0];

                    IFieldable luceneField = null;
                    IFieldable luceneSortedField = null;
                    object parsedVal = null;
                    if (string.IsNullOrEmpty(indexField.Type)) indexField.Type = string.Empty;
                    switch (indexField.Type.ToUpper())
                    {
                        case "NUMBER":
                        case "INT":
                            if (!TryConvert<int>(field.Value, out parsedVal))
                                break;
                            luceneField = new NumericField(field.Key, Field.Store.YES, !Equals(lucenePolicy, Field.Index.NO)).SetIntValue((int)parsedVal);
                            luceneSortedField = new NumericField(SortedFieldNamePrefix + field.Key, Field.Store.NO, true).SetIntValue((int)parsedVal);
                            break;
                        case "FLOAT":
                            if (!TryConvert<float>(field.Value, out parsedVal))
                                break;
                            luceneField = new NumericField(field.Key, Field.Store.YES, !Equals(lucenePolicy, Field.Index.NO)).SetFloatValue((float)parsedVal);
                            luceneSortedField = new NumericField(SortedFieldNamePrefix + field.Key, Field.Store.NO, true).SetFloatValue((float)parsedVal);
                            break;
                        case "DOUBLE":
                            if (!TryConvert<double>(field.Value, out parsedVal))
                                break;
                            luceneField = new NumericField(field.Key, Field.Store.YES, !Equals(lucenePolicy, Field.Index.NO)).SetDoubleValue((double)parsedVal);
                            luceneSortedField = new NumericField(SortedFieldNamePrefix + field.Key, Field.Store.NO, true).SetDoubleValue((double)parsedVal);
                            break;
                        case "LONG":
                            if (!TryConvert<long>(field.Value, out parsedVal))
                                break;
                            luceneField = new NumericField(field.Key, Field.Store.YES, !Equals(lucenePolicy, Field.Index.NO)).SetLongValue((long)parsedVal);
                            luceneSortedField = new NumericField(SortedFieldNamePrefix + field.Key, Field.Store.NO, true).SetLongValue((long)parsedVal);
                            break;
                        case "DATE":
                        case "DATETIME":
                            {
                                SetDateTimeField(field.Key, field.Value, DateTools.Resolution.MILLISECOND, lucenePolicy, ref luceneField, ref luceneSortedField);
                                break;
                            }
                        case "DATE.YEAR":
                            {
                                SetDateTimeField(field.Key, field.Value, DateTools.Resolution.YEAR, lucenePolicy, ref luceneField, ref luceneSortedField);
                                break;
                            }
                        case "DATE.MONTH":
                            {
                                SetDateTimeField(field.Key, field.Value, DateTools.Resolution.MONTH, lucenePolicy, ref luceneField, ref luceneSortedField);
                                break;
                            }
                        case "DATE.DAY":
                            {
                                SetDateTimeField(field.Key, field.Value, DateTools.Resolution.DAY, lucenePolicy, ref luceneField, ref luceneSortedField);
                                break;
                            }
                        case "DATE.HOUR":
                            {
                                SetDateTimeField(field.Key, field.Value, DateTools.Resolution.HOUR, lucenePolicy, ref luceneField, ref luceneSortedField);
                                break;
                            }
                        case "DATE.MINUTE":
                            {
                                SetDateTimeField(field.Key, field.Value, DateTools.Resolution.MINUTE, lucenePolicy, ref luceneField, ref luceneSortedField);
                                break;
                            }
                        default:
                            luceneField =
                                new Field(field.Key,
                                    field.Value,
                                    Field.Store.YES,
                                    lucenePolicy,
                                          Equals(lucenePolicy, Field.Index.NO) ? Field.TermVector.NO : Field.TermVector.YES
                                );
                            luceneSortedField = new Field(SortedFieldNamePrefix + field.Key,
                                                    field.Value,
                                                    Field.Store.NO, //we don't want to store the field because we're only using it to sort, not return data
                                                    Field.Index.NOT_ANALYZED,
                                                    Field.TermVector.NO
                                );
                            break;
                    }

                    //if the parsed value is null, this means it couldn't parse and we should log this error
                    if (luceneField == null)
                    {
                        OnIndexingError(new IndexingErrorEventArgs("Could not parse value: " + field.Value + "into the type: " + indexField.Type, nodeId, null));
                    }
                    else
                    {
                        d.Add(luceneField);

                        //add the special sorted field if sorting is enabled
                        if (indexField.EnableSorting)
                        {
                            d.Add(luceneSortedField);
                        }
                    }
                }
            }

            AddSpecialFieldsToDocument(d, fields);

            var docArgs = new DocumentWritingEventArgs(nodeId, d, fields);
            OnDocumentWriting(docArgs);
            if (docArgs.Cancel)
                return;

            writer.UpdateDocument(new Term(IndexNodeIdFieldName, nodeId.ToString(CultureInfo.InvariantCulture)), d);

            OnNodeIndexed(new IndexedNodeEventArgs(nodeId));
        }

        
        private void SetDateTimeField(string fieldName, string valueToParse, DateTools.Resolution resolution, Field.Index lucenePolicy,
            ref IFieldable field, ref IFieldable sortedField)
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
                          Equals(lucenePolicy, Field.Index.NO) ? Field.TermVector.NO : Field.TermVector.YES
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
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
				//we want to store the nodeId separately as it's the index
				{IndexNodeIdFieldName, allValuesForIndexing[IndexNodeIdFieldName]},
				//add the index type first
				{IndexTypeFieldName, allValuesForIndexing[IndexTypeFieldName]}
            };
        }

        /// <summary>
        /// Process all of the queue items
        /// </summary>
        protected internal void SafelyProcessQueueItems()
        {
            if (!RunAsync)
            {
                StartIndexing();
            }
            else
            {
                if (!_isIndexing)
                {
                    //don't run the worker if it's currently running since it will just pick up the rest of the queue during its normal operation                    
                    lock (_indexingLocker)
                    {
                        if (!_isIndexing && (_asyncTask == null || _asyncTask.IsCompleted))
                        {
                            //Trace.WriteLine("Examine: Launching task");
                            if (!_cancellationTokenSource.IsCancellationRequested)
                            {
                                _asyncTask = Task.Factory.StartNew(
                                    () =>
                                    {
                                        //Ensure the indexing processes is using an invariant culture
                                        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                                        StartIndexing();
                                    },
                                    _cancellationTokenSource.Token,  //use our cancellation token
                                    TaskCreationOptions.None,
                                    TaskScheduler.Default).ContinueWith(task =>
                                    {
                                        if (task.IsCanceled)
                                        {
                                            //if this gets cancelled, we need to ... ?
                                        }
                                    });
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Processes the queue and checks if optimization needs to occur at the end
        /// </summary>
        void StartIndexing()
        {
            if (!_isIndexing)
            {
                lock (_indexingLocker)
                {
                    if (!_isIndexing && !_cancellationTokenSource.IsCancellationRequested)
                    {

                        _isIndexing = true;

                        //keep processing until it is complete
                        var numProcessedItems = 0;
                        do
                        {
                            numProcessedItems = ForceProcessQueueItems();
                        } while (numProcessedItems > 0);

                        //reset the flag
                        _isIndexing = false;

                        OnIndexOperationComplete(new EventArgs());
                    }
                }
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
            return ForceProcessQueueItems(false);
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
        /// The 'block' parameter is very important, normally this will not block since we're running on a background thread anyways, however
        /// during app shutdown we want to process the remaining queue and block.
        /// </remarks>
        
        private int ForceProcessQueueItems(bool block)
        {
            if (!IndexExists())
            {
                return 0;
            }

            //check if the index is ready to be written to.
            if (!IndexReady())
            {
                OnIndexingError(new IndexingErrorEventArgs("Cannot index queue items, the index is currently locked", -1, null));
                return 0;
            }

            //track all of the nodes indexed
            var indexedNodes = new List<IndexedNode>();

            Interlocked.Increment(ref _activeWrites);

            try
            {
                var writer = GetIndexWriter();

                if (block)
                {
                    if (!_indexQueue.IsAddingCompleted)
                    {
                        throw new InvalidOperationException("Cannot block unless the queue is finalized");
                    }

                    foreach (var batch in _indexQueue.GetConsumingEnumerable())
                    {
                        foreach (var item in batch)
                        {
                            ProcessQueueItem(item, indexedNodes, writer);
                        }
                    }
                }
                else
                {
                    IEnumerable<IndexOperation> batch;
                    //index while we're not cancelled and while there's items in there
                    while (!_cancellationTokenSource.IsCancellationRequested && _indexQueue.TryTake(out batch))
                    {
                        foreach (var item in batch)
                        {
                            ProcessQueueItem(item, indexedNodes, writer);
                        }
                    }
                }

                //this is required to ensure the index is written to during the same thread execution
                // if we are in blocking mode, the do the wait
                if (!RunAsync || block)
                {
                    //commit the changes (this will process the deletes too)
                    writer.Commit();

                    writer.WaitForMerges();
                }
                else
                {
                    _committer.ScheduleCommit();
                }

                if (indexedNodes.Count > 0)
                {
                    //raise the completed event
                    OnNodesIndexed(new IndexedNodesEventArgs(IndexerData, indexedNodes));
                }
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("Error indexing queue items", -1, ex));
            }
            finally
            {
                Interlocked.Decrement(ref _activeWrites);
            }

            return indexedNodes.Count;
        }

        /// <summary>
        /// This queues up a commit for the index so that a commit doesn't happen on every individual write since that is quite expensive
        /// </summary>
        private class IndexCommiter : DisposableObject
        {
            private readonly LuceneIndexer _indexer;
            private DateTime _timestamp;
            private Timer _timer;
            private readonly object _locker = new object();
            private const int WaitMilliseconds = 2000;

            /// <summary>
            /// The maximum time period that will elapse until we must commit (5 mins)
            /// </summary>
            private const int MaxWaitMilliseconds = 300000;

            public IndexCommiter(LuceneIndexer indexer)
            {
                _indexer = indexer;
            }

            
            public void ScheduleCommit()
            {
                lock (_locker)
                {
                    if (_timer == null)
                    {
                        //if we've been cancelled then be sure to commit now
                        if (_indexer._cancellationTokenSource.IsCancellationRequested)
                        {
                            //perform the commit
                            _indexer._writer?.Commit();
                        }
                        else
                        {
                            //It's the initial call to this at the beginning or after successful commit
                            _timestamp = DateTime.Now;
                            _timer = new Timer(_ => TimerRelease());
                            _timer.Change(WaitMilliseconds, 0);
                        }
                    }
                    else
                    {
                        //if we've been cancelled then be sure to cancel the timer and commit now
                        if (_indexer._cancellationTokenSource.IsCancellationRequested)
                        {
                            //Stop the timer
                            _timer.Change(Timeout.Infinite, Timeout.Infinite);
                            _timer.Dispose();
                            _timer = null;

                            //perform the commit
                            _indexer._writer?.Commit();
                        }
                        else if (
                            // must be less than the max
                            DateTime.Now - _timestamp < TimeSpan.FromMilliseconds(MaxWaitMilliseconds) &&
                            // and less than the delay
                            DateTime.Now - _timestamp < TimeSpan.FromMilliseconds(WaitMilliseconds))
                        {
                            //Delay  
                            _timer.Change(WaitMilliseconds, 0);
                        }
                        else
                        {
                            //Cannot delay! the callback will execute on the pending timeout
                        }
                    }
                }
            }

            
            private void TimerRelease()
            {
                lock (_locker)
                {
                    //if the timer is not null then a commit has been scheduled
                    if (_timer != null)
                    {
                        //Stop the timer
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                        _timer.Dispose();
                        _timer = null;

                        //perform the commit
                        _indexer._writer?.Commit();
                    }
                }
            }

            protected override void DisposeResources()
            {
                TimerRelease();
            }
        }

        
        private void ProcessQueueItem(IndexOperation item, ICollection<IndexedNode> indexedNodes, IndexWriter writer)
        {
            switch (item.Operation)
            {
                case IndexOperationType.Add:

                    if (ValidateDocument(item.Item.DataToIndex))
                    {
                        //var added = ProcessIndexQueueItem(item, inMemoryWriter);
                        var added = ProcessIndexQueueItem(item, writer);
                        indexedNodes.Add(added);
                    }
                    else
                    {
                        //do the delete but no commit - it may or may not exist in the index but since it is not 
                        // valid it should definitely not be there.
                        ProcessDeleteQueueItem(item, writer, false);

                        OnIgnoringNode(new IndexingNodeDataEventArgs(item.Item.DataToIndex, int.Parse(item.Item.Id), null, item.Item.IndexType));
                    }
                    break;
                case IndexOperationType.Delete:
                    ProcessDeleteQueueItem(item, writer, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Queues an indexing operation
        /// </summary>
        /// <param name="op"></param>
        protected void EnqueueIndexOperation(IndexOperation op)
        {
            //don't queue if there's been a cancellation requested
            if (!_cancellationTokenSource.IsCancellationRequested && !_indexQueue.IsAddingCompleted)
            {
                _indexQueue.Add(new[] { op });
            }
            else
            {
                OnIndexingError(
                    new IndexingErrorEventArgs(
                        "App is shutting down so index operation is ignored: " + op.Item.Id, -1, null));
            }
        }

        /// <summary>
        /// Queues an indexing operation batch
        /// </summary>
        /// <param name="ops"></param>
        protected void EnqueueIndexOperation(IEnumerable<IndexOperation> ops)
        {
            //don't queue if there's been a cancellation requested
            if (!_cancellationTokenSource.IsCancellationRequested && !_indexQueue.IsAddingCompleted)
            {
                _indexQueue.Add(ops);
            }
            else
            {
                OnIndexingError(
                    new IndexingErrorEventArgs(
                        "App is shutting down so index batch operation is ignored", -1, null));
            }
        }

        /// <summary>
        /// Initialize the directory
        /// </summary>
        
        private void InitializeDirectory()
        {
            if (_directory != null) return;

            if (DirectoryFactory == null)
            {
                //ensure all of the folders are created at startup   
                VerifyFolder(WorkingFolder);
                VerifyFolder(LuceneIndexFolder);
                _directory = DirectoryTracker.Current.GetDirectory(LuceneIndexFolder);
            }
            _directory = DirectoryTracker.Current.GetDirectory(LuceneIndexFolder, InvokeDirectoryFactory);
        }

        /// <summary>
        /// Purely to do with stupid medium trust
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        
        private Directory InvokeDirectoryFactory(string s)
        {
            return DirectoryFactory.CreateDirectory(this, s);
        }

        
        private Directory _directory;

        /// <summary>
        /// Gets the <see cref="IDirectoryFactory"/> if one is being used
        /// </summary>
        public IDirectoryFactory DirectoryFactory { get; private set; }

        /// <summary>
        /// Returns the Lucene Directory used to store the index
        /// </summary>
        /// <returns></returns>
        
        public virtual Directory GetLuceneDirectory()
        {
            return _writer != null ? _writer.Directory : _directory;
        }

        private FileStream _logOutput;

        /// <summary>
        /// Used to create an index writer - this is called in GetIndexWriter (and therefore, GetIndexWriter should not be overridden)
        /// </summary>
        /// <returns></returns>
        
        protected virtual IndexWriter CreateIndexWriter()
        {
            var writer = WriterTracker.Current.GetWriter(
                GetLuceneDirectory(),
                WriterFactory);

#if FULLDEBUG
            //If we want to enable logging of lucene output....
            //It is also possible to set a default InfoStream on the static IndexWriter class            
            _logOutput?.Close();
            if (LuceneIndexFolder != null)
            {
                try
                {
                    _logOutput = new FileStream(Path.Combine(LuceneIndexFolder.FullName, DateTime.UtcNow.ToString("yyyy-MM-dd") + ".log"), FileMode.Append);
                    var w = new StreamWriter(_logOutput);
                    writer.SetInfoStream(w);
                }
                catch (Exception)
                {
                    //if an exception is thrown here we won't worry about it, it will mean we cannot create the log file
                }
            }
            
#endif

            return writer;
        }

        /// <summary>
        /// Purely to do with stupid medium trust
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        
        private IndexWriter WriterFactory(Directory d)
        {
            if (d == null) throw new ArgumentNullException(nameof(d));
            var writer = new IndexWriter(d, IndexingAnalyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
            return writer;
        }

        /// <summary>
        /// Returns an index writer for the current directory
        /// </summary>
        /// <returns></returns>
        
        public IndexWriter GetIndexWriter()
        {
            EnsureIndex(false);

            if (_writer == null)
            {
                Monitor.Enter(_writerLocker);
                try
                {
                    if (_writer == null)
                    {
                        _writer = CreateIndexWriter();
                    }
                }
                finally
                {
                    Monitor.Exit(_writerLocker);
                }

            }

            return _writer;
        }

        #endregion

            #region Private

            /// <summary>
            /// Stupid medium trust - that is the only reason this method exists
            /// </summary>
            /// <returns></returns>
        
        private LuceneSearcher GetSearcher()
        {
            return new LuceneSearcher(GetIndexWriter(), IndexingAnalyzer);
        }

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
                var convertFrom = tc.ConvertFrom(null, CultureInfo.InvariantCulture, val);
                if (convertFrom != null)
                {
                    parsedVal = (T)convertFrom;
                    return true;
                }
                else
                {
                    parsedVal = null;
                    return false;
                }
            }
            catch (NotSupportedException)
            {
                parsedVal = null;
                return false;
            }

        }

        /// <summary>
        /// Adds 'special' fields to the Lucene index for use internally.
        /// By default this will add the __IndexType and __NodeId fields to the Lucene Index both specified by:
        /// - Field.Store.YES
        /// - Field.Index.NOT_ANALYZED_NO_NORMS
        /// - Field.TermVector.NO
        /// </summary>
        /// <param name="d"></param>
        /// <param name="fields"></param>
        
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
        /// Reads the FileInfo passed in into a dictionary object and deletes it from the index
        /// </summary>
        /// <param name="op"></param>
        /// <param name="iw"></param>
        /// <param name="performCommit"></param>
        
        private void ProcessDeleteQueueItem(IndexOperation op, IndexWriter iw, bool performCommit = true)
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

        
        private IndexedNode ProcessIndexQueueItem(IndexOperation op, IndexWriter writer)
        {
            //get the node id
            var nodeId = int.Parse(op.Item.Id);

            //now, add the index with our dictionary object
            var fields = GetDataToIndex(op.Item.DataToIndex, op.Item.IndexType);
            EnsureSpecialFields(fields, op.Item.Id, op.Item.IndexType);

            AddDocument(fields, writer, nodeId, op.Item.IndexType);

            CommitCount++;

            return new IndexedNode() { NodeId = nodeId, Type = op.Item.IndexType };
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

        private readonly DisposableIndexer _disposer;
        private readonly IndexCommiter _committer;

        private class DisposableIndexer : DisposableObject
        {
            private readonly LuceneIndexer _indexer;

            public DisposableIndexer(LuceneIndexer indexer)
            {
                _indexer = indexer;
            }

            /// <summary>
            /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
            /// </summary>
            
            protected override void DisposeResources()
            {
                
                if (_indexer.WaitForIndexQueueOnShutdown)
                {
                    //if there are active adds, lets way/retry (5 seconds)
                    RetryUntilSuccessOrTimeout(() => _indexer._activeAddsOrDeletes == 0, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));
                }

                //cancel any operation currently in place
                _indexer._cancellationTokenSource.Cancel();

                //ensure nothing more can be added
                _indexer._indexQueue.CompleteAdding();

                if (_indexer._writer != null)
                {
                    if (_indexer.WaitForIndexQueueOnShutdown)
                    {
                        //process remaining items and block until complete
                        _indexer.ForceProcessQueueItems(true);
                    }
                }

                //dispose it now
                _indexer._indexQueue.Dispose();

                //Don't close the writer until there are definitely no more writes
                //NOTE: we are not taking into acccount the WaitForIndexQueueOnShutdown property here because we really want to make sure
                //we are not terminating Lucene while it is actively writing to the index.
                RetryUntilSuccessOrTimeout(() => _indexer._activeWrites == 0, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(1));

                //close the committer, this will ensure a final commit is made if one has been queued
                _indexer._committer.Dispose();

                _indexer._writer?.Dispose();

                _indexer._cancellationTokenSource.Dispose();

                _indexer._logOutput?.Close();
            }

            private static bool RetryUntilSuccessOrTimeout(Func<bool> task, TimeSpan timeout, TimeSpan pause)
            {

                if (pause.TotalMilliseconds < 0)
                {
                    throw new ArgumentException("pause must be >= 0 milliseconds");
                }
                var stopwatch = Stopwatch.StartNew();
                do
                {
                    if (task()) { return true; }
                    Thread.Sleep((int)pause.TotalMilliseconds);
                }
                while (stopwatch.Elapsed < timeout);
                return false;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_internalSearcher.IsValueCreated)
            {
                _internalSearcher.Value.Dispose();
            }
            _disposer.Dispose();
        }

        #endregion


    }
}

