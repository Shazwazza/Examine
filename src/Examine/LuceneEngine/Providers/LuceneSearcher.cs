using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using Examine;
using Examine.SearchCriteria;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Analysis;
using Directory = Lucene.Net.Store.Directory;


namespace Examine.LuceneEngine.Providers
{
    ///<summary>
    /// Standard object used to search a Lucene index
    ///</summary>
    public class LuceneSearcher : BaseLuceneSearcher, IDisposable
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        
        public LuceneSearcher()
        {
            _disposer = new DisposableSearcher(this);
            _reopener = new ReaderReopener(this);
        }
        
        /// <summary>
        /// Constructor allowing for creating a NRT instance based on a given writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="analyzer"></param>
        
        public LuceneSearcher(IndexWriter writer, Analyzer analyzer)
            : base(analyzer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            _disposer = new DisposableSearcher(this);
            _reopener = new ReaderReopener(this);
            _nrtWriter = writer;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="workingFolder"></param>
        /// <param name="analyzer"></param>
        
        public LuceneSearcher(DirectoryInfo workingFolder, Analyzer analyzer)
            : base(analyzer)
        {
            _disposer = new DisposableSearcher(this);
            _reopener = new ReaderReopener(this);
            LuceneIndexFolder = new DirectoryInfo(Path.Combine(workingFolder.FullName, "Index"));
            InitializeDirectory();
        }

        /// <summary>
        /// Constructor to allow creating an indexer at runtime with the specified lucene directory
        /// </summary>
        /// <param name="luceneDirectory"></param>
        /// <param name="analyzer"></param>
        
        public LuceneSearcher(Lucene.Net.Store.Directory luceneDirectory, Analyzer analyzer)
            : base(analyzer)
        {
            _disposer = new DisposableSearcher(this);
            _reopener = new ReaderReopener(this);
            LuceneIndexFolder = null;
            _directory = luceneDirectory;
        }

        #endregion      

        /// <summary>
        /// Initialize the directory
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">
        /// this will throw if a Directory instance is not found at the specified location</exception>
        /// <remarks>
        /// It is assumed that the indexer for the searcher will always be initialized first and therefore a directory would be available
        /// </remarks>
        
        private void InitializeDirectory()
        {
            if (_directory != null) return;

            _directory = DirectoryTracker.Current.GetDirectory(LuceneIndexFolder, true);
        }

        /// <summary>
        /// Used as a singleton instance
        /// </summary>
        private IndexSearcher _searcher;
        private volatile IndexReader _reader;
        private readonly object _locker = new object();
        private Directory _directory;
        private IndexWriter _nrtWriter;
        private bool? _exists;
        private bool _disposed = false;
        private ReaderReopener _reopener;

        /// <summary>
        /// Do not access this object directly. The public property ensures that the folder state is always up to date
        /// </summary>
        private DirectoryInfo _indexFolder;
        
        /// <summary>
        /// Initializes the provider.
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

            //need to check if the index set is specified, if it's not, we'll see if we can find one by convension
            //if the folder is not null and the index set is null, we'll assume that this has been created at runtime.
            //NOTE: Don't proceed if the _luceneDirectory is set since we already know where to look.
            if (config["indexSet"] == null && (LuceneIndexFolder == null && _directory == null))
            {
                //if we don't have either, then we'll try to set the index set by naming convensions
                var found = false;
                if (name.EndsWith("Searcher"))
                {
                    var setNameByConvension = name.Remove(name.LastIndexOf("Searcher")) + "IndexSet";
                    //check if we can assign the index set by naming convension
                    var set = IndexSets.Instance.Sets.Cast<IndexSet>().SingleOrDefault(x => x.SetName == setNameByConvension);

                    if (set != null)
                    {
                        set.ReplaceTokensInIndexPath();

                        //we've found an index set by naming convensions :)
                        IndexSetName = set.SetName;
                        found = true;
                    }
                }

                if (!found)
                    throw new ArgumentNullException("indexSet on LuceneExamineIndexer provider has not been set in configuration");

                //get the folder to index
                LuceneIndexFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));
            }
            else if (config["indexSet"] != null && _directory == null)
            {
                if (IndexSets.Instance.Sets[config["indexSet"]] == null)
                    throw new ArgumentException("The indexSet specified for the LuceneExamineIndexer provider does not exist");

                IndexSetName = config["indexSet"];

                var indexSet = IndexSets.Instance.Sets[IndexSetName];

                indexSet.ReplaceTokensInIndexPath();

                //get the folder to index
                LuceneIndexFolder = new DirectoryInfo(Path.Combine(indexSet.IndexDirectory.FullName, "Index"));
            }

            InitializeDirectory();
        }

        /// <summary>
        /// Directory where the Lucene.NET Index resides
        /// </summary>
        public DirectoryInfo LuceneIndexFolder
        {
            get
            {
                if (_indexFolder == null)
                {
                    return null;
                }

                //ensure's that it always up to date!
                _indexFolder.Refresh();
                return _indexFolder;
            }
            private set
            {
                _indexFolder = value;
            }
        }
        
        /// <summary>
        /// Name of the Lucene.NET index set
        /// </summary>
        public string IndexSetName { get; private set; }

        /// <summary>
        /// Gets the searcher for this instance, this method will also ensure that the searcher is up to date whenever this method is called.
        /// </summary>
        /// <returns>
        /// Returns null if the underlying index doesn't exist
        /// </returns>
        
        public override Searcher GetLuceneSearcher()
        {
            if (!ValidateSearcher()) return null;

            //ensure scoring is turned on for sorting
            _searcher.SetDefaultFieldSortScoring(true, true);
            return _searcher;
        }

        public override ICriteriaContext GetCriteriaContext()
        {
            var indexFieldValueTypes = FieldValueTypes.Current.GetIndexFieldValueTypes(GetLuceneDirectory());
            return new CriteriaContext(indexFieldValueTypes, GetLuceneSearcher());
        }

        /// <summary>
        /// Returns a list of fields to search on
        /// </summary>
        /// <returns></returns>
        
        protected internal override string[] GetSearchFields()
        {
            if (!ValidateSearcher()) return new string[] {};

            //var reader = _searcherIndexReader;
            var fields = _reader.GetFieldNames(IndexReader.FieldOption.ALL);
            //exclude the special index fields
            var searchFields = fields
                .Where(x => !x.StartsWith(LuceneIndexer.SpecialFieldPrefix))
                .ToArray();
            return searchFields;
        }

        
        protected virtual Directory GetLuceneDirectory()
        {
            if (_nrtWriter != null)
            {
                return _nrtWriter.Directory;
            }
            
            return _directory;
        }

        /// <summary>
        /// Used to open a new reader when first initializing, when forcing a re-open or when the reader becomes stale (new data is in the index)
        /// </summary>
        /// <returns></returns>
        
        protected virtual IndexReader OpenNewReader()
        {
            //If a writer was resolved, we can now operate in NRT mode
            // this will be successful only if the index writer has been initialized
            // on the associated indexer before this method is called
            if (TryEstablishNrtReader())
            {
                _nrtWriter.GetReader();
            }

            //If we cannot resolve an existing writer, we'll fallback to opening a normal
            // non-nrt reader. When this reader becomes stale, the above will check again 
            // if an NRT reader can be resolved.
            return IndexReader.Open(GetLuceneDirectory(), true);
        }

        /// <summary>
        /// This will check if a writer exists for the current directory to see if we can establish an NRT reader in the future
        /// </summary>
        /// <returns></returns>
        
        private bool TryEstablishNrtReader()
        {
            //Try to resolve an existing IndexWriter for the current directory
            if (_nrtWriter == null)
            {
                _nrtWriter = WriterTracker.Current.GetWriter(GetLuceneDirectory());
            }

            return _nrtWriter != null;
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
        /// This checks if the IndexSearcher is initialized and up to date.
        /// </summary>
        
        private bool ValidateSearcher()
        {
            //can't proceed if there's no index
            if (!IndexExistsImpl()) return false;

            //TODO: Would be nicer if this used LazyInitializer instead of double check locking

            //if there isn't a reader yet, we need to create one for this index
            if (_reader == null)
            {
                lock (_locker)
                {
                    //double check
                    if (_reader == null)
                    {
                        try
                        {
                            //get a reader - could be NRT or based on directly depending on how this was constructed
                            _reader = _nrtWriter == null
                                ? OpenNewReader()
                                : _nrtWriter.GetReader();

                            _searcher = new IndexSearcher(_reader);

                            //track it!
                            OpenReaderTracker.Current.AddOpenReader(_reader);
                        }
                        catch (IOException ex)
                        {
                            throw new ApplicationException("Could not create an index searcher with the supplied lucene directory", ex);
                        }
                    }
                }
            }
            else
            {
                //a reader exists, check if it's time to re-open it
                _reopener.ScheduleReopen();
            }

            return true;
        }

        /// <summary>
        /// This schedules a reader to be re-opened
        /// </summary>
        /// <remarks>
        /// Checking a reader's status is expensive because it iterates all index files (lots of IO) and since
        /// that should happen all of the time (i.e. every search we might need to re-open) we need to be a little
        /// bit smart about doing it. So instead of checking every time there is a search, we'll have a sliding 
        /// timer to check if it's been xyz amount of time before it was last checked, and if so then we'll perform the
        /// check, otherwise we'll keep the current reader with it's current status. This timeout can be quite small but 
        /// it could save on quite a lot of overhead if there are tons of searches per page (i.e. Umbraco media queries).
        /// When we have an NRT reader open however, we don't need to worry about the status check, we can 'just' check 
        /// and reopen which can execute without the IO overhead.
        /// 
        /// http://blog.mikemccandless.com/2011/11/near-real-time-readers-with-lucenes.html
        /// http://blog.mikemccandless.com/2011/09/lucenes-searchermanager-simplifies.html
        /// 
        /// The last remarks here is that once a reader becomes NRT, we don't really have to worry about the timers, it's also
        /// interesting to note that until a reader becomes NRT it means that in theory nothing has been written to the index
        /// and therefore a lot of this timer stuff is unecessary... BUT! The timer stuff does guarantee that the reader becomes
        /// NRT quicker and doesn't have to wait for a stale reader to execute so we'll leave the timer stuff in here since
        /// once a writer is created and an index is updated, the current reader won't become NRT immediately without these
        /// timers.
        /// </remarks>
        private class ReaderReopener : DisposableObject
        {
            private readonly LuceneSearcher _luceneSearcher;
            private DateTime _timestamp;
            private Timer _timer;
            private readonly object _locker = new object();
            private bool _isLongPoll = false;
            private const int WaitMilliseconds = 2000; //only wait 2 seconds to check

            /// <summary>
            /// The maximum time period that will elapse until we must check
            /// </summary>
            private const int MaxWaitMilliseconds = 10000; //10 seconds

            /// <summary>
            /// Used when not running in NRT mode and searching is idle
            /// </summary>
            private const int LongPollWaitMilliseconds = 300000; //10 mins

            public ReaderReopener(LuceneSearcher indexer)
            {
                _luceneSearcher = indexer;
            }

            
            public void ScheduleReopen()
            {
                lock (_locker)
                {
                    var wasLongPoll = _isLongPoll;
                    _isLongPoll = false;

                    if (_timer == null)
                    {
                        //if we've been disposed
                        if (_luceneSearcher._disposed)
                        {
                            
                        }
                        else
                        {
                            //if we're on NRT then we should just try to re-open
                            if (_luceneSearcher._nrtWriter != null)
                            {
                                MaybeReopen();
                            }
                            else
                            {
                                //It's the initial call to this at the beginning or after successful commit
                                _timestamp = DateTime.Now;
                                _timer = new Timer(_ => TimerRelease());
                                _timer.Change(WaitMilliseconds, 0);
                                _isLongPoll = false;
                            }
                        }
                    }
                    else
                    {
                        //if we've been disposed then be sure to cancel the timer
                        if (_luceneSearcher._disposed)
                        {
                            //Stop the timer
                            StopTimer();

                        }
                        else if (wasLongPoll)
                        {                          
                            //if we are currently in long polling mode and we've been requested to check, then 
                            //lets see how long we've waited for and if it's overdue then we'll perform the check
                            if (DateTime.Now - _timestamp > TimeSpan.FromMilliseconds(WaitMilliseconds))
                            {
                                //we were in the middle of long polling and we've exceeded the minimum wait time so let's force 
                                //checking/reopening before the search continues to ensure it's up to date
                                StopTimer();
                                MaybeReopen();
                                //re-start long poll
                                StartLongPoll();
                            }
                            else
                            {
                                //if we were in long poll mode and we've been requested to re-check but it's been less
                                // than the min time, then change the timer to the min time
                                _timer.Change(WaitMilliseconds, 0);
                            }
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
                    _isLongPoll = false;

                    //if the timer is not null then a commit has been scheduled
                    if (_timer != null)
                    {
                        //Stop the timer
                        StopTimer();

                        if (!_luceneSearcher._disposed)
                        {
                            MaybeReopen();

                            //after this expires, we need to put this check on a longer timer if 
                            // we are not running on an NRT so that the next search will most likely be 
                            // up to date instead of receiving potentially very stale data (i.e. the first 
                            // search after a large commit would be stale because we don't actively check 
                            // to re-open until 2 seconds after the search)
                            StartLongPoll();
                        }
                    }
                }
            }

            private void StopTimer()
            {
                //Stop the timer
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _timer = null;
            }

            protected override void DisposeResources()
            {
                lock (_locker)
                {
                    //if the timer is not null then a commit has been scheduled
                    if (_timer != null)
                    {
                        //Stop the timer
                        StopTimer();
                    }
                }
            }

            
            private void StartLongPoll()
            {
                //if no timer, we are not NRT and we are not currently in long poll mode then
                // the force long poll to start
                if (_timer == null && !_isLongPoll && _luceneSearcher._nrtWriter == null)
                {
                    _timestamp = DateTime.Now;
                    _timer = new Timer(_ => TimerRelease());
                    _timer.Change(LongPollWaitMilliseconds, 0);
                    _isLongPoll = true;
                }
            }

            
            private void MaybeReopen()
            {
                switch (_luceneSearcher._reader.GetReaderStatus())
                {
                    case ReaderStatus.Current:
                        break;
                    case ReaderStatus.Closed:
                        lock (_locker)
                        {
                            //get a reader - could be NRT or based on directly depending on how this was constructed
                            _luceneSearcher._reader = _luceneSearcher._nrtWriter == null
                                ? _luceneSearcher.OpenNewReader()
                                : _luceneSearcher._nrtWriter.GetReader();

                            _luceneSearcher._searcher = new IndexSearcher(_luceneSearcher._reader);

                            //track it!
                            OpenReaderTracker.Current.AddOpenReader(_luceneSearcher._reader);
                        }
                        break;
                    case ReaderStatus.NotCurrent:

                        lock (_locker)
                        {
                            IndexReader newReader;

                            //Here we'll check if we are not running in NRT mode, this will be the case
                            // if the indexer hasn't created a writer. But if it has, we want to become NRT so 
                            // we'll check if we can.
                            if (_luceneSearcher._nrtWriter == null && _luceneSearcher.TryEstablishNrtReader())
                            {
                                //the new reader will now be an NRT reader
                                newReader = _luceneSearcher._nrtWriter.GetReader();
                            }
                            else
                            {
                                //yes, this is actually the way the Lucene wants you to work...
                                //normally, i would have thought just calling Reopen() on the underlying reader would suffice... but it doesn't.
                                //here's references: 
                                // http://stackoverflow.com/questions/1323779/lucene-indexreader-reopen-doesnt-seem-to-work-correctly
                                // http://gist.github.com/173978 
                                //Also note that when a new reader is returned from Reopen() the old reader is not actually closed - 
                                // but more importantly the old reader might still be in use from another thread! So we can't just 
                                // close it here because that would cause a YSOD: Lucene.Net.Store.AlreadyClosedException: this IndexReader is closed
                                // since another thread might be using it. I'm 'hoping' that the GC will just take care of the left over reader's that might
                                // be currently being used in a search, otherwise there's really no way to now when it's safe to close the reader. 

                                newReader = _luceneSearcher._reader.Reopen();
                            }

                            if (newReader != _luceneSearcher._reader)
                            {
                                //if it's changed, then re-assign, note: the above, before we used to close the old one here
                                // but that will cause problems since the old reader might be in use on another thread.
                                _luceneSearcher._reader = newReader;
                                _luceneSearcher._searcher = new IndexSearcher(_luceneSearcher._reader);

                                //track it!
                                OpenReaderTracker.Current.AddOpenReader(_luceneSearcher._reader);

                                //get rid of old ones (anything a minute or older)
                                OpenReaderTracker.Current.CloseStaleReaders(_luceneSearcher.GetLuceneDirectory(), TimeSpan.FromMinutes(1));
                            }
                        }

                        break;
                }
            }
        }

        #region IDisposable Members

        private readonly DisposableSearcher _disposer;

        private class DisposableSearcher : DisposableObject
        {
            private readonly LuceneSearcher _searcher;

            public DisposableSearcher(LuceneSearcher searcher)
            {
                _searcher = searcher;
            }

            /// <summary>
            /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
            /// </summary>
            
            protected override void DisposeResources()
            {
                _searcher._disposed = true;
                if (_searcher._reader != null)
                {
                    try
                    {
                        //this will close if there are no readers remaining, otherwise if there 
                        // are readers remaining they will be auto-shut down based on the DecrementReaderResult
                        // that would still have it in use (i.e. this actually just called DecRef underneath)
                        _searcher._reader.Close();
                    }
                    catch (AlreadyClosedException)
                    {
                        //if this happens, more than one instance has decreased referenced, this could occur if the 
                        // DecrementReaderResult never disposed, which occurs if people don't actually iterate the 
                        // result collection.
                    }
                }
                //close the reopener
                _searcher._reopener.Dispose();                
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _disposer.Dispose();
        }

        #endregion
    }

}

