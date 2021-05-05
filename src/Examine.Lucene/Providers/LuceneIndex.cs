using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Microsoft.Extensions.Logging;
using Directory = Lucene.Net.Store.Directory;
using static Lucene.Net.Index.IndexWriter;
using Microsoft.Extensions.Options;
using Lucene.Net.Analysis.Standard;
using Examine.Lucene.Indexing;
using Examine.Lucene.Directories;

namespace Examine.Lucene.Providers
{

    ///<summary>
    /// Abstract object containing all of the logic used to use Lucene as an indexer
    ///</summary>
    public class LuceneIndex : BaseIndexProvider, IDisposable, IIndexStats
    {
        #region Constructors

        /// <summary>
        /// Constructor to create an indexer
        /// </summary>
        public LuceneIndex(
            ILoggerFactory loggerFactory,
            string name,
            IOptionsSnapshot<LuceneDirectoryIndexOptions> indexOptions)
           : base(loggerFactory, name, indexOptions)
        {
            _committer = new IndexCommiter(this);
            _logger = loggerFactory.CreateLogger<LuceneIndex>();

            LuceneIndexFolder = null;

            LuceneDirectoryIndexOptions namedOptions = indexOptions.Get(name);

            if (namedOptions == null)
            {
                throw new InvalidOperationException($"No named {typeof(LuceneDirectoryIndexOptions)} options with name {name}");
            }

            DefaultAnalyzer = namedOptions.Analyzer ?? new StandardAnalyzer(LuceneInfo.CurrentVersion);
            if (namedOptions.DirectoryFactory == null)
            {
                throw new InvalidOperationException($"No {typeof(IDirectoryFactory)} assigned");
            }
            _directory = namedOptions.DirectoryFactory.CreateDirectory(name);

            //initialize the field types
            _fieldValueTypeCollection = new Lazy<FieldValueTypeCollection>(() => CreateFieldValueTypes(namedOptions.IndexValueTypesFactory));

            _searcher = new Lazy<LuceneSearcher>(CreateSearcher);
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        //TODO: The problem with this is that the writer would already need to be configured with a PerFieldAnalyzerWrapper
        // with all of the field definitions in place, etc... but that will most likely never happen
        /// <summary>
        /// Constructor to allow for creating an indexer at runtime - using NRT
        /// </summary>
        internal LuceneIndex(
            ILoggerFactory loggerFactory,
            string name,
            IOptionsSnapshot<LuceneIndexOptions> indexOptions,
            IndexWriter writer)
               : base(loggerFactory, name, indexOptions)
        {
            _committer = new IndexCommiter(this);
            _logger = loggerFactory.CreateLogger<LuceneIndex>();

            _writer = new TrackingIndexWriter(writer ?? throw new ArgumentNullException(nameof(writer)));
            DefaultAnalyzer = writer.Analyzer;

            //initialize the field types
            _fieldValueTypeCollection = new Lazy<FieldValueTypeCollection>(
                () => CreateFieldValueTypes(indexOptions.GetNamedOptions(name).IndexValueTypesFactory));

            LuceneIndexFolder = null;
            _searcher = new Lazy<LuceneSearcher>(CreateSearcher);
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }


        #endregion

        private PerFieldAnalyzerWrapper _fieldAnalyzer;
        private ControlledRealTimeReopenThread<IndexSearcher> _nrtReopenThread;
        private readonly ILogger<LuceneIndex> _logger;
        private readonly Directory _directory;
        private FileStream _logOutput;
        private bool _disposedValue;
        private readonly IndexCommiter _committer;

        private volatile TrackingIndexWriter _writer;

        private int _activeWrites = 0;

        /// <summary>
        /// Used for creating the background tasks
        /// </summary>
        private readonly object _taskLocker = new object();

        /// <summary>
        /// Used to aquire the index writer
        /// </summary>
        private readonly object _writerLocker = new object();

        private readonly Lazy<LuceneSearcher> _searcher;

        private bool? _exists;

        /// <summary>
        /// Gets a searcher for the index
        /// </summary>
        public override ISearcher Searcher => _searcher.Value;

        /// <summary>
        /// The async task that runs during an async indexing operation
        /// </summary>
        private Task _asyncTask = Task.CompletedTask;

        /// <summary>
        /// Used to cancel the async operation
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Gets the token from the token source
        /// </summary>
        private CancellationToken _cancellationToken;

        private readonly Lazy<FieldValueTypeCollection> _fieldValueTypeCollection;

        // tracks the latest Generation value of what has been indexed.This can be used to force update a searcher to this generation.
        private long? _latestGen;

        #region Properties

        /// <summary>
        /// Returns the <see cref="FieldValueTypeCollection"/> configured for this index
        /// </summary>
        public FieldValueTypeCollection FieldValueTypeCollection => _fieldValueTypeCollection.Value;

        /// <summary>
        /// The default analyzer to use when indexing content, by default, this is set to StandardAnalyzer
        /// </summary>
        public Analyzer DefaultAnalyzer { get; }

        public PerFieldAnalyzerWrapper FieldAnalyzer => _fieldAnalyzer
            ?? (_fieldAnalyzer =
                (DefaultAnalyzer is PerFieldAnalyzerWrapper pfa)
                    ? pfa
                    : new PerFieldAnalyzerWrapper(DefaultAnalyzer));

        /// <summary>
        /// Used to keep track of how many index commits have been performed.
        /// This is used to determine when index optimization needs to occur.
        /// </summary>
        public int CommitCount { get; protected internal set; }

        /// <summary>
        /// Indicates whether or this system will process the queue items asynchonously - used for testing
        /// </summary>
        public bool RunAsync { get; protected internal set; } = true;

        /// <summary>
        /// The folder that stores the Lucene Index files
        /// </summary>
        public DirectoryInfo LuceneIndexFolder { get; protected set; }

        /// <summary>
        /// This should ONLY be used internally by the scheduled committer we should refactor this out in the future
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected bool IsCancellationRequested => _cancellationToken.IsCancellationRequested;

        #endregion

        #region Events

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
        protected override void OnIndexingError(IndexingErrorEventArgs e)
        {
            base.OnIndexingError(e);

            if (!RunAsync)
            {
                var msg = "Indexing Error Occurred: " + e.Message;
                throw new Exception(msg, e.Exception);
            }

        }

        protected virtual void OnDocumentWriting(DocumentWritingEventArgs docArgs)
            => DocumentWriting?.Invoke(this, docArgs);

        #endregion

        #region Provider implementation

        protected override void PerformIndexItems(IEnumerable<ValueSet> values, Action<IndexOperationEventArgs> onComplete)
        {
            // need to lock, we don't want to issue any node writing if there's an index rebuild occuring
            lock (_writerLocker)
            {
                var currentToken = _cancellationToken;

                if (RunAsync)
                {
                    QueueTask(() => PerformIndexItemsInternal(values, currentToken), onComplete, currentToken);
                }
                else
                {
                    var count = 0;
                    try
                    {
                        count = PerformIndexItemsInternal(values, currentToken);
                    }
                    finally
                    {
                        onComplete?.Invoke(new IndexOperationEventArgs(this, count));
                    }
                }
            }
        }

        private int PerformIndexItemsInternal(IEnumerable<ValueSet> valueSets, CancellationToken cancellationToken)
        {
            //check if the index is ready to be written to.
            if (!IndexReady())
            {
                OnIndexingError(new IndexingErrorEventArgs(this, "Cannot index queue items, the index is currently locked", null, null));
                return 0;
            }

            //track all of the nodes indexed
            var indexedNodes = 0;

            Interlocked.Increment(ref _activeWrites);

            try
            {
                var writer = IndexWriter;

                foreach (var valueSet in valueSets)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var op = new IndexOperation(valueSet, IndexOperationType.Add);
                    if (ProcessQueueItem(op, writer))
                    {
                        indexedNodes++;
                    }
                }

                //this is required to ensure the index is written to during the same thread execution
                if (!RunAsync)
                {
                    //commit the changes (this will process the deletes too)
                    writer.IndexWriter.Commit();
                }
                else
                {
                    _committer.ScheduleCommit();
                }
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs(this, "Error indexing queue items", null, ex));
            }
            finally
            {
                Interlocked.Decrement(ref _activeWrites);
            }

            return indexedNodes;
        }

        /// <summary>
        /// Creates a brand new index, this will override any existing index with an empty one
        /// </summary>
        public void EnsureIndex(bool forceOverwrite)
        {
            if (!forceOverwrite && _exists.HasValue && _exists.Value)
            {
                return;
            }

            var indexExists = IndexExists();
            if (!indexExists || forceOverwrite)
            {
                //if we can't acquire the lock exit - this will happen if this method is called multiple times but we don't want this 
                // logic to actually execute multiple times
                if (Monitor.TryEnter(_writerLocker))
                {
                    try
                    {
                        var dir = GetLuceneDirectory();

                        if (!indexExists)
                        {
                            _logger.LogDebug("Initializing new index");

                            //if there's no index, we need to create one
                            CreateNewIndex(dir);
                        }
                        else
                        {
                            //it does exists so we'll need to clear it out

                            _logger.LogDebug("Clearing existing index");

                            if (_writer == null)
                            {
                                //This will happen if the writer hasn't been created/initialized yet which
                                // might occur if a rebuild is triggered before any indexing has been triggered.
                                //In this case we need to initialize a writer and continue as normal.
                                //Since we are already inside the writer lock and it is null, we are allowed to 
                                // make this call with out using GetIndexWriter() to do the initialization.
                                _writer = CreateIndexWriterInternal();
                            }

                            //We're forcing an overwrite, 
                            // this means that we need to cancel all operations currently in place,
                            // clear the queue and delete all of the data in the index.

                            //cancel any operation currently in place
                            _cancellationTokenSource.Cancel();

                            // indicates that it was locked, this generally shouldn't happen but we don't want to have unhandled exceptions
                            if (_writer == null)
                            {
                                _logger.LogWarning("writer was null, exiting");
                                return;
                            }

                            try
                            {
                                //remove all of the index data
                                _latestGen = _writer.DeleteAll();
                                _writer.IndexWriter.Commit();
                            }
                            finally
                            {
                                _cancellationTokenSource.Dispose();
                                _cancellationTokenSource = new CancellationTokenSource();
                                _cancellationToken = _cancellationTokenSource.Token;

                                // we need to reset this task because if any faults occur when rebuilding
                                // the task will remain in a canceled state and nothing will ever run again.
                                _asyncTask = Task.CompletedTask;
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
                    OnIndexingError(new IndexingErrorEventArgs(this, "Could not acquire lock in EnsureIndex so cannot create new index", null, null));
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
                if (IsLocked(dir))
                {
                    //unlock it!
                    Unlock(dir);
                }
                //create the writer (this will overwrite old index files)
                var writerConfig = new IndexWriterConfig(LuceneInfo.CurrentVersion, FieldAnalyzer)
                {
                    OpenMode = OpenMode.CREATE,
                    MergeScheduler = new ErrorLoggingConcurrentMergeScheduler(Name,
                        (s, e) => OnIndexingError(new IndexingErrorEventArgs(this, s, "-1", e)))
                };
                writer = new IndexWriter(dir, writerConfig);

            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs(this, "An error occurred creating the index", null, ex));
                return;
            }
            finally
            {
                writer?.Dispose();
                _exists = true;
            }
        }


        /// <summary>
        /// Creates a new index, any existing index will be deleted
        /// </summary>
        public override void CreateIndex()
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                OnIndexingError(new IndexingErrorEventArgs(this, "Cannot create a new index, indexing cancellation has been requested", null, null));
                return;
            }
            EnsureIndex(true);
        }

        /// <summary>
        /// Deletes a node from the index.                
        /// </summary>
        /// <remarks>
        /// When a content node is deleted, we also need to delete it's children from the index so we need to perform a 
        /// custom Lucene search to find all decendents and create Delete item queues for them too.
        /// </remarks>
        /// <param name="itemIds">ID of the node to delete</param>
        /// <param name="onComplete"></param>
        protected override void PerformDeleteFromIndex(IEnumerable<string> itemIds, Action<IndexOperationEventArgs> onComplete)
        {
            // need to lock, we don't want to issue any node writing if there's an index rebuild occuring
            lock (_writerLocker)
            {
                var currentToken = _cancellationToken;

                if (RunAsync)
                {
                    QueueTask(() => PerformDeleteFromIndexInternal(itemIds, currentToken), onComplete, currentToken);
                }
                else
                {
                    var count = 0;
                    try
                    {
                        count = PerformDeleteFromIndexInternal(itemIds, currentToken);
                    }
                    finally
                    {
                        onComplete?.Invoke(new IndexOperationEventArgs(this, count));
                    }
                }
            }
        }

        private int PerformDeleteFromIndexInternal(IEnumerable<string> itemIds, CancellationToken cancellationToken)
        {
            //check if the index is ready to be written to.
            if (!IndexReady())
            {
                OnIndexingError(new IndexingErrorEventArgs(this, "Cannot index queue items, the index is currently locked", null, null));
                return 0;
            }

            //track all of the nodes indexed
            var indexedNodes = 0;

            Interlocked.Increment(ref _activeWrites);

            try
            {
                var writer = IndexWriter;

                foreach (var id in itemIds)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var op = new IndexOperation(new ValueSet(id), IndexOperationType.Delete);
                    if (ProcessQueueItem(op, writer))
                    {
                        indexedNodes++;
                    }
                }

                //this is required to ensure the index is written to during the same thread execution
                if (!RunAsync)
                {
                    //commit the changes (this will process the deletes too)
                    writer.IndexWriter.Commit();
                }
                else
                {
                    _committer.ScheduleCommit();
                }
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs(this, "Error indexing queue items", null, ex));
            }
            finally
            {
                Interlocked.Decrement(ref _activeWrites);
            }

            return indexedNodes;
        }

        #endregion

        #region Protected



        /// <summary>
        /// Creates the <see cref="FieldValueTypeCollection"/> for this index
        /// </summary>
        /// <param name="indexValueTypesFactory"></param>
        /// <returns></returns>
        protected virtual FieldValueTypeCollection CreateFieldValueTypes(IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypesFactory = null)
        {
            //copy to writable dictionary
            var defaults = new Dictionary<string, IFieldValueTypeFactory>();
            foreach (var defaultIndexValueType in ValueTypeFactoryCollection.GetDefaultValueTypes(LoggerFactory, DefaultAnalyzer))
            {
                defaults[defaultIndexValueType.Key] = defaultIndexValueType.Value;
            }
            //copy the factory over the defaults
            if (indexValueTypesFactory != null)
            {
                foreach (var value in indexValueTypesFactory)
                {
                    defaults[value.Key] = value.Value;
                }
            }

            var result = new FieldValueTypeCollection(FieldAnalyzer, defaults, FieldDefinitions);
            return result;
        }

        /// <summary>
        /// Checks if the index is ready to open/write to.
        /// </summary>
        /// <returns></returns>
        protected bool IndexReady() => _writer != null || (!IsLocked(GetLuceneDirectory()));

        /// <summary>
        /// Check if there is an index in the index folder
        /// </summary>
        /// <returns></returns>

        public override bool IndexExists() => _writer != null || IndexExistsImpl();

        /// <summary>
        /// Check if the index is readable/healthy
        /// </summary>
        /// <returns></returns>
        public bool IsReadable(out Exception ex)
        {
            if (_writer != null)
            {
                try
                {
                    using (_writer.IndexWriter.GetReader(false))
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
                using (DirectoryReader.Open(GetLuceneDirectory()))
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
            if (_exists.HasValue && _exists.Value)
                return true;

            //if it's not been set or it just doesn't exist, re-read the lucene files
            if (!_exists.HasValue || !_exists.Value)
            {
                _exists = DirectoryReader.IndexExists(GetLuceneDirectory());
            }

            return _exists.Value;
        }



        /// <summary>
        /// Removes the specified term from the index
        /// </summary>
        /// <param name="indexTerm"></param>
        /// <param name="iw"></param>
        /// <param name="performCommit"></param>
        /// <returns>Boolean if it successfully deleted the term, or there were on errors</returns>
        private bool DeleteFromIndex(Term indexTerm, TrackingIndexWriter iw, bool performCommit = true)
        {
            string itemId = null;
            if (indexTerm.Field == "id")
            {
                itemId = indexTerm.Text();
            }

            try
            {
                //if the index doesn't exist, then no don't attempt to open it.
                if (!IndexExists())
                {
                    return true;
                }

                iw.DeleteDocuments(indexTerm);

                if (performCommit)
                {
                    iw.IndexWriter.Commit();
                }

                return true;
            }
            catch (Exception ee)
            {
                OnIndexingError(new IndexingErrorEventArgs(this, "Error deleting Lucene index", itemId, ee));
                return false;
            }
        }

        private static IEnumerable<KeyValuePair<string, List<object>>> CopyDictionary(IDictionary<string, List<object>> d)
        {
            var result = new KeyValuePair<string, List<object>>[d.Count];
            d.CopyTo(result, 0);
            return result;
        }

        /// <summary>
        /// Collects the data for the fields and adds the document which is then committed into Lucene.Net's index
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="valueSet">The data to index.</param>
        /// <param name="writer">The writer that will be used to update the Lucene index.</param>
        protected virtual void AddDocument(Document doc, ValueSet valueSet, TrackingIndexWriter writer)
        {
            _logger.LogDebug("Write lucene doc id:{DocumentId}, category:{DocumentCategory}, type:{DocumentItemType}",
                valueSet.Id,
                valueSet.Category,
                valueSet.ItemType);

            //add node id
            IIndexFieldValueType nodeIdValueType = FieldValueTypeCollection.GetValueType(ExamineFieldNames.ItemIdFieldName, FieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.Raw));
            nodeIdValueType.AddValue(doc, valueSet.Id);

            //add the category
            IIndexFieldValueType categoryValueType = FieldValueTypeCollection.GetValueType(ExamineFieldNames.CategoryFieldName, FieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.InvariantCultureIgnoreCase));
            categoryValueType.AddValue(doc, valueSet.Category);

            //add the item type
            IIndexFieldValueType indexTypeValueType = FieldValueTypeCollection.GetValueType(ExamineFieldNames.ItemTypeFieldName, FieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.InvariantCultureIgnoreCase));
            indexTypeValueType.AddValue(doc, valueSet.ItemType);

            //copy to a new dictionary, there has been cases of an exception "Collection was modified; enumeration operation may not execute."
            // TODO: This is because ValueSet can be shared between indexes (same value set passed to each)
            // we should remove this since this will cause mem overheads and it's not actually going to fix the problem since it's only copying
            // this dictionary but not the entire value set
            foreach (KeyValuePair<string, List<object>> field in CopyDictionary(valueSet.Values))
            {
                //check if we have a defined one
                if (FieldDefinitions.TryGetValue(field.Key, out FieldDefinition definedFieldDefinition))
                {
                    IIndexFieldValueType valueType = FieldValueTypeCollection.GetValueType(
                        definedFieldDefinition.Name,
                        FieldValueTypeCollection.ValueTypeFactories.TryGetFactory(definedFieldDefinition.Type, out var valTypeFactory)
                            ? valTypeFactory
                            : FieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.FullText));

                    foreach (var o in field.Value)
                    {
                        valueType.AddValue(doc, o);
                    }
                }
                else if (field.Key.StartsWith(ExamineFieldNames.SpecialFieldPrefix))
                {
                    //Check for the special field prefix, if this is the case it's indexed as an invariant culture value

                    IIndexFieldValueType valueType = FieldValueTypeCollection.GetValueType(field.Key, FieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.InvariantCultureIgnoreCase));
                    foreach (var o in field.Value)
                    {
                        valueType.AddValue(doc, o);
                    }
                }
                else
                {
                    // wasn't specifically defined, use FullText as the default

                    IIndexFieldValueType valueType = FieldValueTypeCollection.GetValueType(
                        field.Key,
                        FieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.FullText));

                    foreach (var o in field.Value)
                    {
                        valueType.AddValue(doc, o);
                    }
                }
            }

            var docArgs = new DocumentWritingEventArgs(valueSet, doc);
            OnDocumentWriting(docArgs);
            if (docArgs.Cancel)
            {
                return;
            }

            // TODO: try/catch with OutOfMemoryException (see docs on UpdateDocument), though i've never seen this in real life
            _latestGen = writer.UpdateDocument(new Term(ExamineFieldNames.ItemIdFieldName, valueSet.Id), doc);
        }

        /// <summary>
        /// This queues up a commit for the index so that a commit doesn't happen on every individual write since that is quite expensive
        /// </summary>
        private class IndexCommiter : DisposableObjectSlim
        {
            private readonly LuceneIndex _index;
            private DateTime _timestamp;
            private Timer _timer;
            private readonly object _locker = new object();
            private const int WaitMilliseconds = 2000;

            /// <summary>
            /// The maximum time period that will elapse until we must commit (5 mins)
            /// </summary>
            private const int MaxWaitMilliseconds = 300000;

            public IndexCommiter(LuceneIndex index)
            {
                _index = index;
            }


            public void ScheduleCommit()
            {
                lock (_locker)
                {
                    if (_timer == null)
                    {
                        //if we've been cancelled then be sure to commit now
                        if (_index.IsCancellationRequested)
                        {
                            //perform the commit
                            _index._writer?.IndexWriter?.Commit();
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
                        if (_index.IsCancellationRequested)
                        {
                            //Stop the timer
                            _timer.Change(Timeout.Infinite, Timeout.Infinite);
                            _timer.Dispose();
                            _timer = null;

                            //perform the commit
                            _index._writer?.IndexWriter?.Commit();
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

                        try
                        {
                            //perform the commit
                            _index._writer?.IndexWriter?.Commit();
                        }
                        catch (Exception e)
                        {
                            // It is unclear how/why this happens but probably indicates index corruption
                            // see https://github.com/Shazwazza/Examine/issues/164
                            _index.OnIndexingError(new IndexingErrorEventArgs(
                                _index,
                                "An error occurred during the index commit operation, if this error is persistent then index rebuilding is necessary",
                                "-1",
                                e));
                        }
                    }
                }
            }

            protected override void DisposeResources() => TimerRelease();
        }


        private bool ProcessQueueItem(IndexOperation item, TrackingIndexWriter writer)
        {
            switch (item.Operation)
            {
                case IndexOperationType.Add:

                    var added = ProcessIndexQueueItem(item, writer);
                    return added;
                case IndexOperationType.Delete:
                    ProcessDeleteQueueItem(item, writer, false);
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Returns the Lucene Directory used to store the index
        /// </summary>
        /// <returns></returns>
        public Directory GetLuceneDirectory() => _writer != null ? _writer.IndexWriter.Directory : _directory;

        /// <summary>
        /// Used to create an index writer - this is called in GetIndexWriter (and therefore, GetIndexWriter should not be overridden)
        /// </summary>
        /// <returns></returns>
        private TrackingIndexWriter CreateIndexWriterInternal()
        {
            Directory dir = GetLuceneDirectory();

            // Unfortunatley if the appdomain is taken down this will remain locked, so we can 
            // ensure that it's unlocked here in that case.
            try
            {
                if (IsLocked(dir))
                {
                    //unlock it!
                    Unlock(dir);
                }
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs(this, "The index was locked and could not be unlocked", null, ex));
                return null;
            }

            IndexWriter writer = CreateIndexWriter(dir);

            var trackingIndexWriter = new TrackingIndexWriter(writer);

            return trackingIndexWriter;
        }

        /// <summary>
        /// Method that creates the IndexWriter
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        protected virtual IndexWriter CreateIndexWriter(Directory d)
        {
            if (d == null)
            {
                throw new ArgumentNullException(nameof(d));
            }

            var writer = new IndexWriter(d, new IndexWriterConfig(LuceneInfo.CurrentVersion, FieldAnalyzer)
            {

#if FULLDEBUG

            //If we want to enable logging of lucene output....
            //It is also possible to set a default InfoStream on the static IndexWriter class
            InfoStream =

            _logOutput?.Close();
            if (LuceneIndexFolder != null)
            {
                try
                {
                    System.IO.Directory.CreateDirectory(LuceneIndexFolder.FullName);
                    _logOutput = new FileStream(Path.Combine(LuceneIndexFolder.FullName, DateTime.UtcNow.ToString("yyyy-MM-dd") + ".log"), FileMode.Append);
           
            
                }
                catch (Exception ex)
                {
                    //if an exception is thrown here we won't worry about it, it will mean we cannot create the log file
                }
            }

#endif          

                MergeScheduler = new ErrorLoggingConcurrentMergeScheduler(Name,
                    (s, e) => OnIndexingError(new IndexingErrorEventArgs(this, s, "-1", e)))
            });

            return writer;
        }

        /// <summary>
        /// Gets the TrackingIndexWriter for the current directory
        /// </summary>
        /// <remarks>
        /// Using a TrackingIndexWriter allows for more control over NRT readers. Though Examine doesn't specifically
        /// use the features of TrackingIndexWriter directly (i.e. to be able to wait for a specific generation),
        /// this is a requirement of NRT with SearchManager and ControlledRealTimeReopenThread.
        /// See example: http://www.lucenetutorial.com/lucene-nrt-hello-world.html
        /// http://blog.mikemccandless.com/2011/11/near-real-time-readers-with-lucenes.html
        /// https://stackoverflow.com/questions/17993960/lucene-4-4-0-new-controlledrealtimereopenthread-sample-usage
        /// TODO: Do we need/want to use the ControlledRealTimeReopenThread? Else according to mikecandles above in comments
        /// we can probably just get away with using MaybeReopen each time we search. Though there are comments in the lucene
        /// code to avoid that and do that on a background thread, which is exactly what ControlledRealTimeReopenThread already does.
        /// </remarks>
        public TrackingIndexWriter IndexWriter
        {
            get
            {
                EnsureIndex(false);

                if (_writer == null)
                {
                    Monitor.Enter(_writerLocker);
                    try
                    {
                        if (_writer == null)
                        {
                            _writer = CreateIndexWriterInternal();
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_writerLocker);
                    }

                }

                return _writer;
            }
        }

        #endregion

        #region Private

        private LuceneSearcher CreateSearcher()
        {
            var possibleSuffixes = new[] { "Index", "Indexer" };
            var name = Name;
            foreach (var suffix in possibleSuffixes)
            {
                //trim the "Indexer" / "Index" suffix if it exists
                if (!name.EndsWith(suffix))
                    continue;
                name = name.Substring(0, name.LastIndexOf(suffix, StringComparison.Ordinal));
            }

            TrackingIndexWriter writer = IndexWriter;
            var searcherManager = new SearcherManager(writer.IndexWriter, true, null);

            _nrtReopenThread = new ControlledRealTimeReopenThread<IndexSearcher>(writer, searcherManager, 5.0, 0.1)
            {
                Name = "NRT Reopen Thread"
            };

            _nrtReopenThread.Start();

            // wait for most recent changes when first creating the searcher
            WaitForChanges();

            return new LuceneSearcher(name + "Searcher", searcherManager, FieldAnalyzer, FieldValueTypeCollection);
        }


        /// <summary>
        /// Deletes the item from the index either by id or by category
        /// </summary>
        /// <param name="op"></param>
        /// <param name="iw"></param>
        /// <param name="performCommit"></param>
        private void ProcessDeleteQueueItem(IndexOperation op, TrackingIndexWriter iw, bool performCommit = true)
        {

            //if the id is empty then remove the whole type
            if (!string.IsNullOrEmpty(op.ValueSet.Id))
            {
                DeleteFromIndex(new Term(ExamineFieldNames.ItemIdFieldName, op.ValueSet.Id), iw, performCommit);
            }
            else if (!string.IsNullOrEmpty(op.ValueSet.Category))
            {
                DeleteFromIndex(new Term(ExamineFieldNames.CategoryFieldName, op.ValueSet.Category), iw, performCommit);
            }

            CommitCount++;
        }


        private bool ProcessIndexQueueItem(IndexOperation op, TrackingIndexWriter writer)
        {

            //raise the event and assign the value to the returned data from the event
            var indexingNodeDataArgs = new IndexingItemEventArgs(this, op.ValueSet);
            OnTransformingIndexValues(indexingNodeDataArgs);
            if (indexingNodeDataArgs.Cancel)
                return false;

            var d = new Document();
            AddDocument(d, op.ValueSet, writer);

            CommitCount++;

            return true;
        }

        private void QueueTask(Func<int> op, Action<IndexOperationEventArgs> onComplete, CancellationToken currentToken)
        {
            using (ExecutionContext.SuppressFlow())
            {
                // This can be called by many threads and we want to keep a linear
                // chain of continuations so we lock.
                lock (_taskLocker)
                {
                    if (_asyncTask.IsCanceled)
                    {
                        _logger.LogDebug("Indexing cancellation requested, cannot proceed");
                        onComplete?.Invoke(new IndexOperationEventArgs(this, 0));
                    }
                    else if (_asyncTask.IsFaulted)
                    {
                        _logger.LogDebug($"Previous task was faulted with exception {_asyncTask.Exception.ToString() ?? "NULL"}");
                        onComplete?.Invoke(new IndexOperationEventArgs(this, 0));
                    }
                    else
                    {
                        _logger.LogDebug("Queuing a new background thread");

                        // The task is initialized to completed so just continue with
                        // and return the new task so that any new appended tasks are the current
                        Task t = _asyncTask.ContinueWith(
                            x =>
                            {
                                var indexedCount = 0;
                                try
                                {
                                    // execute the callback
                                    indexedCount = op();
                                }
                                finally
                                {
                                    // when the task is done call the complete callback
                                    onComplete?.Invoke(new IndexOperationEventArgs(this, indexedCount));
                                }
                            },
                            currentToken,
                            // This ensures that all callbacks passed are executed, not just the first.
                            // See https://blog.stephencleary.com/2015/01/a-tour-of-task-part-7-continuations.html
                            // - need to explicitly define TaskContinuationOptions.DenyChildAttach + TaskScheduler.Default
                            TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.DenyChildAttach,
                            TaskScheduler.Default);

                        if (t.IsCanceled)
                        {
                            _logger.LogDebug("Task was cancelled before it began");
                            onComplete?.Invoke(new IndexOperationEventArgs(this, 0));
                        }
                        else if (t.IsFaulted)
                        {
                            _logger.LogDebug(_asyncTask.Exception, $"Task was cancelled before it began");
                            onComplete?.Invoke(new IndexOperationEventArgs(this, 0));
                        }

                        // make this task the current one
                        _asyncTask = t;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Blocks the calling thread until the internal searcher can see latest documents
        /// </summary>
        /// <remarks>
        /// Useful if you want a searcher to see the very latest changes. Typically this is not used and the searchers
        /// refresh on a near real time schedule.
        /// </remarks>
        public void WaitForChanges()
        {
            if (_latestGen.HasValue)
            {
                _nrtReopenThread?.WaitForGeneration(_latestGen.Value);
            }
        }

        /// <summary>
        /// Used to force the index into asynchronous or synchronous index processing
        /// </summary>
        /// <returns></returns>
        public IDisposable WithThreadingMode(IndexThreadingMode mode) => new ForceThreadingModeIndexProcessor(this, mode);

        /// <summary>
        /// Used to force the index into synchronous index processing
        /// </summary>
        private class ForceThreadingModeIndexProcessor : DisposableObjectSlim
        {
            private readonly LuceneIndex _index;
            private readonly IndexThreadingMode _mode;
            private readonly bool _orig;

            public ForceThreadingModeIndexProcessor(LuceneIndex index, IndexThreadingMode mode)
            {
                _index = index;
                _mode = mode;
                _orig = _index.RunAsync;
                _index.RunAsync = _mode == IndexThreadingMode.Asynchronous;
            }

            protected override void DisposeResources() => _index.RunAsync = _orig;
        }

        public Task<long> GetDocumentCountAsync()
        {
            var writer = IndexWriter;
            return Task.FromResult((long)writer.IndexWriter.NumDocs);
        }

        public Task<IEnumerable<string>> GetFieldNamesAsync()
        {
            var writer = IndexWriter;
            using (var reader = writer.IndexWriter.GetReader(false))
            {
                IEnumerable<string> fieldInfos = MultiFields.GetMergedFieldInfos(reader).Select(x => x.Name);
                return Task.FromResult(fieldInfos);
            }
        }

        private bool RetryUntilSuccessOrTimeout(Func<bool> task, TimeSpan timeout, TimeSpan pause, string timeoutMsg)
        {
            if (pause.TotalMilliseconds < 0)
            {
                throw new ArgumentException("pause must be >= 0 milliseconds");
            }
            var stopwatch = Stopwatch.StartNew();
            do
            {
                if (task())
                {
                    return true;
                }

                Thread.Sleep((int)pause.TotalMilliseconds);
            }
            while (stopwatch.Elapsed < timeout);

            _logger.LogInformation(timeoutMsg);
            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_nrtReopenThread != null)
                    {
                        _nrtReopenThread.Interrupt();
                        _nrtReopenThread.Dispose();
                    }

                    if (_searcher.IsValueCreated)
                    {
                        _searcher.Value.Dispose();
                    }

                    //cancel any operation currently in place
                    _cancellationTokenSource.Cancel();

                    //Don't close the writer until there are definitely no more writes
                    //NOTE: we are not taking into acccount the WaitForIndexQueueOnShutdown property here because we really want to make sure
                    //we are not terminating Lucene while it is actively writing to the index.
                    RetryUntilSuccessOrTimeout(() => _activeWrites == 0, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1), "Timeout elapsed waiting on final writes during shutdown.");

                    //close the committer, this will ensure a final commit is made if one has been queued
                    _committer.Dispose();

                    if (_writer != null && !_writer.IndexWriter.IsClosed)
                    {
                        try
                        {
                            _writer?.IndexWriter?.Dispose(true);
                        }
                        catch (Exception e)
                        {
                            OnIndexingError(new IndexingErrorEventArgs(this, "Error closing the index", "-1", e));
                        }

                        try
                        {
                            _writer?.IndexWriter?.Analyzer.Dispose();
                        }
                        catch (ObjectDisposedException)
                        {
                            // Swallow. If the LuceneIndex was created with a writer and it externally has already been
                            // disposed, this will throw since any accesss to properties on the writer will throw.
                            // This should not happen with the check for IsClosed though.
                        }
                        catch (Exception e)
                        {
                            OnIndexingError(new IndexingErrorEventArgs(this, "Error closing the index analyzer", "-1", e));
                        }
                    }

                    _cancellationTokenSource.Dispose();

                    _logOutput?.Close();
                }
                _disposedValue = true;
            }
        }

        public void Dispose() => Dispose(disposing: true);
    }


}

