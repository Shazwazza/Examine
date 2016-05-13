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
        [SecuritySafeCritical]
        public LuceneSearcher()
        {
            _disposer = new DisposableSearcher(this);
            _directoryLazy = new Lazy<Directory>(InitializeDirectory);
        }
        
        /// <summary>
        /// Constructor allowing for creating a NRT instance based on a given writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="analyzer"></param>
        [SecuritySafeCritical]
        public LuceneSearcher(IndexWriter writer, Analyzer analyzer)
            : base(analyzer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            _disposer = new DisposableSearcher(this);
            _nrtWriter = writer;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="workingFolder"></param>
        /// <param name="analyzer"></param>
        [SecuritySafeCritical]
        public LuceneSearcher(DirectoryInfo workingFolder, Analyzer analyzer)
            : base(analyzer)
        {
            _disposer = new DisposableSearcher(this);
            LuceneIndexFolder = new DirectoryInfo(Path.Combine(workingFolder.FullName, "Index"));
            _directoryLazy = new Lazy<Directory>(InitializeDirectory);
            _directoryLazy = new Lazy<Directory>(InitializeDirectory);
        }

        /// <summary>
        /// Constructor to allow creating an indexer at runtime with the specified lucene directory
        /// </summary>
        /// <param name="luceneDirectory"></param>
        /// <param name="analyzer"></param>
        [SecuritySafeCritical]
        public LuceneSearcher(Lucene.Net.Store.Directory luceneDirectory, Analyzer analyzer)
            : base(analyzer)
        {
            _disposer = new DisposableSearcher(this);
            LuceneIndexFolder = null;
            _directoryExplicit = luceneDirectory;
            _directoryLazy = new Lazy<Directory>(InitializeDirectory);
        }

        #endregion      

        /// <summary>
        /// NOTE: This is all to do with stupid medium trust
        /// </summary>
        /// <returns></returns>
        [SecuritySafeCritical]
        private Directory InitializeDirectory()
        {
            if (_directoryExplicit != null)
                return _directoryExplicit;
            return DirectoryTracker.Current.GetDirectory(LuceneIndexFolder, true);
        }

        /// <summary>
        /// Used as a singleton instance
        /// </summary>
        private IndexSearcher _searcher;
        private volatile IndexReader _reader;
        private readonly object _locker = new object();
        private readonly Lazy<Lucene.Net.Store.Directory> _directoryLazy;
        private readonly Lucene.Net.Store.Directory _directoryExplicit;
        private readonly IndexWriter _nrtWriter;

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
        [SecuritySafeCritical]
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            //need to check if the index set is specified, if it's not, we'll see if we can find one by convension
            //if the folder is not null and the index set is null, we'll assume that this has been created at runtime.
            //NOTE: Don't proceed if the _luceneDirectory is set since we already know where to look.
            if (config["indexSet"] == null && (LuceneIndexFolder == null && _directoryExplicit == null))
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
            else if (config["indexSet"] != null && _directoryExplicit == null)
            {
                if (IndexSets.Instance.Sets[config["indexSet"]] == null)
                    throw new ArgumentException("The indexSet specified for the LuceneExamineIndexer provider does not exist");

                IndexSetName = config["indexSet"];

                var indexSet = IndexSets.Instance.Sets[IndexSetName];

                indexSet.ReplaceTokensInIndexPath();

                //get the folder to index
                LuceneIndexFolder = new DirectoryInfo(Path.Combine(indexSet.IndexDirectory.FullName, "Index"));
            }
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
        /// Ensures the index exists
        /// </summary>
        [SecuritySafeCritical]
        [Obsolete("This is not used and performs no operation, if no index directory exists for the searcher the searcher should just return empty results", true)]
        public virtual void EnsureIndex()
        {
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
        [SecuritySafeCritical]
        public override Searcher GetSearcher()
        {
            if (!ValidateSearcher(false)) return null;

            //ensure scoring is turned on for sorting
            _searcher.SetDefaultFieldSortScoring(true, true);
            return _searcher;
        }

        /// <summary>
        /// Returns a list of fields to search on
        /// </summary>
        /// <returns></returns>
        [SecuritySafeCritical]
        protected internal override string[] GetSearchFields()
        {
            if (!ValidateSearcher(false)) return new string[] {};

            //var reader = _searcher.GetIndexReader();
            var fields = _reader.GetFieldNames(IndexReader.FieldOption.ALL);
            //exclude the special index fields
            var searchFields = fields
                .Where(x => !x.StartsWith(LuceneIndexer.SpecialFieldPrefix))
                .ToArray();
            return searchFields;
        }

        [SecuritySafeCritical]
        protected virtual Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            if (_nrtWriter != null)
            {
                return _nrtWriter.GetDirectory();
            }
            
            return _directoryLazy.Value;
        }

        /// <summary>
        /// Used to open a new reader when first initializing, when forcing a re-open or when the reader becomes stale (new data is in the index)
        /// </summary>
        /// <returns></returns>
        [SecuritySafeCritical]
        protected virtual IndexReader OpenNewReader()
        {
            return IndexReader.Open(GetLuceneDirectory(), true);
        }

        /// <summary>
        /// This checks if the singleton IndexSearcher is initialized and up to date.
        /// </summary>
        /// <param name="forceReopen"></param>
        [SecuritySafeCritical]
        private bool ValidateSearcher(bool forceReopen)
        {
            if (!IndexReader.IndexExists(GetLuceneDirectory())) return false;

            if (!forceReopen)
            {
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
                    switch (_reader.GetReaderStatus())
                    {
                        case ReaderStatus.Current:
                            break;
                        case ReaderStatus.Closed:
                            lock (_locker)
                            {
                                //get a reader - could be NRT or based on directly depending on how this was constructed
                                _reader = _nrtWriter == null
                                    ? OpenNewReader()
                                    : _nrtWriter.GetReader();

                                _searcher = new IndexSearcher(_reader);

                                //track it!
                                OpenReaderTracker.Current.AddOpenReader(_reader);
                            }
                            break;
                        case ReaderStatus.NotCurrent:

                            lock (_locker)
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

                                var newReader = _reader.Reopen();
                                if (newReader != _reader)
                                {
                                    //if it's changed, then re-assign, note: the above, before we used to close the old one here
                                    // but that will cause problems since the old reader might be in use on another thread.
                                    _reader = newReader;
                                    _searcher = new IndexSearcher(_reader);

                                    //track it!
                                    OpenReaderTracker.Current.AddOpenReader(_reader);

                                    //get rid of old ones (anything a minute or older)
                                    OpenReaderTracker.Current.CloseStaleReaders(GetLuceneDirectory(), TimeSpan.FromMinutes(1));
                                }
                            }
                           
                            break;
                    }

                }
            }
            else
            {
                if (_reader != null)
                {
                    lock (_locker)
                    {
                        //double check
                        if (_reader != null)
                        {
                            try
                            {
                                _searcher.Close();
                                _reader.Close();
                            }
                            catch (IOException ex)
                            {
                                //this will happen if it's already closed ( i think )
                                Trace.TraceError("Examine: error occurred closing index searcher. {0}", ex);
                            }
                            finally
                            {
                                //set to null in case another call to this method has passed the first lock and is checking for null
                                _searcher = null;
                                _reader = null;
                            }

                            try
                            {
                                //get a reader - could be NRT or based on directly depending on how this was constructed
                                _reader = _nrtWriter == null
                                    ? OpenNewReader()
                                    : _nrtWriter.GetReader();

                                _searcher = new IndexSearcher(_reader);
                            }
                            catch (IOException ex)
                            {
                                throw new ApplicationException("Could not create an index searcher with the supplied lucene directory", ex);
                            }
                        }

                    }
                }
            }

            return true;
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
            [SecuritySafeCritical]
            protected override void DisposeResources()
            {              
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

