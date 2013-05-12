using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security;
using Examine;
using Examine.LuceneEngine.Faceting;
using Examine.SearchCriteria;
using Examine.Session;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Analysis;
using LuceneManager.Infrastructure;


namespace Examine.LuceneEngine.Providers
{
    ///<summary>
	/// Standard object used to search a Lucene index
	///</summary>
    public class LuceneSearcher : BaseLuceneSearcher
	{
		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
        public LuceneSearcher()
		{
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
            LuceneIndexFolder = new DirectoryInfo(Path.Combine(workingFolder.FullName, "Index"));            
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
			LuceneIndexFolder = null;            
			_luceneDirectory = luceneDirectory;
		}

		#endregion

		/// <summary>
		/// Used as a singleton instance
		/// </summary>
		//private volatile IndexSearcher _searcher;

		//private static readonly object Locker = new object();
	    private Lucene.Net.Store.Directory _luceneDirectory;

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
			if (config["indexSet"] == null && (LuceneIndexFolder == null && _luceneDirectory == null)) 
			{
				//if we don't have either, then we'll try to set the index set by naming convensions
				var found = false;
				if (name.EndsWith("Searcher"))
				{
					var setNameByConvension = name.Remove(name.LastIndexOf("Searcher")) + "IndexSet";
					//check if we can assign the index set by naming convension
					var set = IndexSets.Instance.Sets.Cast<IndexSet>()
						.Where(x => x.SetName == setNameByConvension)
						.SingleOrDefault();

					if (set != null)
					{
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
			else if (config["indexSet"] != null && _luceneDirectory == null)
			{
				if (IndexSets.Instance.Sets[config["indexSet"]] == null)
					throw new ArgumentException("The indexSet specified for the LuceneExamineIndexer provider does not exist");

				IndexSetName = config["indexSet"];

				//get the folder to index
				LuceneIndexFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));
			}

		    
		    FacetConfiguration = IndexSets.Instance.Sets[IndexSetName].GetFacetConfiguration();
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
		/// Do not access this object directly. The public property ensures that the folder state is always up to date
		/// </summary>
		private DirectoryInfo _indexFolder;

        private bool _hasIndex = false;

        /// <summary>
        /// Ensures the index exists exists
        /// </summary>
		[SecuritySafeCritical]
        public virtual void EnsureIndex()
        {
            //Searchers don't create indexes.
            return;

            //if (_hasIndex) return;

            //IndexWriter writer = null;
            //try
            //{
            //    if (!IndexReader.IndexExists(GetLuceneDirectory()))
            //    {
            //        lock(Locker)
            //        {
            //            if (!IndexReader.IndexExists(GetLuceneDirectory()))
            //            {
            //                //create the writer (this will overwrite old index files)
            //                writer = new IndexWriter(GetLuceneDirectory(), IndexingAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);        
            //            }
            //        }
            //    }
            //}            
            //finally
            //{
            //    if (writer != null)
            //    {
            //        writer.Close();
            //        writer = null;
            //    }
            //    _hasIndex = true;
            //}

        }

        /// <summary>
        /// A simple search mechanism to search all fields based on an index type.
        /// </summary>
        /// <remarks>
        /// This can be used to do a simple search against an index type instead of the entire index.
        /// 
        /// This will search every field for any words matching in search text. Each word in the search text will be encapsulated 
        /// in a wild card search too.
        /// 
        /// </remarks>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <param name="indexType"></param>
        /// <returns></returns>
        public ISearchResults Search(string searchText, bool useWildcards, string indexType)
        {
            var sc = CreateSearchCriteria(indexType);
            return TextSearchAllFields(searchText, useWildcards, sc);
        }

        /// <summary>
        /// Name of the Lucene.NET index set
        /// </summary>
        public string IndexSetName { get; private set; }

        /// <summary>
        /// Gets the searcher for this instance, this method will also ensure that the searcher is up to date whenever this method is called.
        /// </summary>
        /// <returns></returns>                
		[SecuritySafeCritical]
        public override Searcher GetSearcher()
        {
            ValidateSearcher(false);

            var token = _searcherContext.GetSearcher();
            DisposableCollector.Track(token);

            //ensure scoring is turned on for sorting
            token.Searcher.SetDefaultFieldSortScoring(true, true);            
            return token.Searcher;
        }

        public override ICriteriaContext GetCriteriaContext()
        {
            ValidateSearcher(false);

            return _searcherContext.FacetsLoader;
        }


        /// <summary>
        /// Returns a list of fields to search on
        /// </summary>
        /// <returns></returns>
		[SecuritySafeCritical]
        protected override internal string[] GetSearchFields()
        {
            ValidateSearcher(false);

            using (var s = _searcherContext.GetSearcher())
            {
                var reader = s.Searcher.GetIndexReader();
                var fields = reader.GetFieldNames(IndexReader.FieldOption.ALL);
                //exclude the special index fields
                var searchFields = fields
                    .Where(x => !x.StartsWith(LuceneIndexer.SpecialFieldPrefix))
                    .ToArray();
                return searchFields;
            }
        }

		[SecuritySafeCritical]
        protected virtual Lucene.Net.Store.Directory GetLuceneDirectory()
        {
			if (_luceneDirectory == null)
			{
				_luceneDirectory = new SimpleFSDirectory(LuceneIndexFolder);
			}
			return _luceneDirectory;
        }

        private SearcherContext _searcherContext;

        //TODO: the real searcher should use RAM directory, will be way faster
        // but need to figure out a way to check if the real directory has been updated.
        //protected virtual Lucene.Net.Store.Directory GetInMemoryLuceneDirectory()
        //{
        //    return new RAMDirectory(GetLuceneDirectory());
        //}

        /// <summary>
        /// This checks if the singleton IndexSearcher is initialized and up to date.
        /// </summary>
        /// <param name="forceReopen"></param>
        [SecuritySafeCritical]
        private void ValidateSearcher(bool forceReopen)
        {
            if (_searcherContext == null)
            {
                _searcherContext = SearcherContexts.Instance.GetContext(GetLuceneDirectory());

                if (_searcherContext == null)
                {
                    throw new NotSupportedException("No indexer is defined for the directory");
                }
            }

            if (ExamineSession.RequireImmediateConsistency)
            {
               ExamineSession.WaitForChanges(_searcherContext.Manager);
            }

            //Handled by NrtManager / SearcContext

            //EnsureIndex();
            //    if (!forceReopen)
            //    {
            //        if (_searcher == null)
            //        {
            //            lock (Locker)
            //            {
            //                //double check
            //                if (_searcher == null)
            //                {

            //                    try
            //                    {
            //                        _searcher = new IndexSearcher(GetLuceneDirectory(), true);
            //                    }
            //                    catch (IOException ex)
            //                    {
            //                        throw new ApplicationException("Could not create an index searcher with the supplied lucene directory", ex);
            //                    }
            //                }
            //            }
            //        }
            //        else
            //        {
            //            if (_searcher.GetReaderStatus() != ReaderStatus.Current)
            //            {
            //                lock (Locker)
            //                {
            //                    //double check, now, we need to find out if it's closed or just not current
            //                    switch (_searcher.GetReaderStatus())
            //                    {
            //                        case ReaderStatus.Current:
            //                            break;
            //                        case ReaderStatus.Closed:
            //                            _searcher = new IndexSearcher(GetLuceneDirectory(), true);
            //                            break;
            //                        case ReaderStatus.NotCurrent:

            //                            //yes, this is actually the way the Lucene wants you to work...
            //                            //normally, i would have thought just calling Reopen() on the underlying reader would suffice... but it doesn't.
            //                            //here's references: 
            //                            // http://stackoverflow.com/questions/1323779/lucene-indexreader-reopen-doesnt-seem-to-work-correctly
            //                            // http://gist.github.com/173978 

            //                            var oldReader = _searcher.GetIndexReader();
            //                            var newReader = oldReader.Reopen(true);
            //                            if (newReader != oldReader)
            //                            {
            //                                _searcher.Close();
            //                                oldReader.Close();
            //                                _searcher = new IndexSearcher(newReader);
            //                            }

            //                            break;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        //need to close the searcher and force a re-open

            //        if (_searcher != null)
            //        {
            //            lock (Locker)
            //            {
            //                //double check
            //                if (_searcher != null)
            //                {
            //                    try
            //                    {
            //                        _searcher.Close();
            //                    }
            //                    catch (IOException)
            //                    {
            //                        //this will happen if it's already closed ( i think )
            //                    }
            //                    finally
            //                    {
            //                        //set to null in case another call to this method has passed the first lock and is checking for null
            //                        _searcher = null;
            //                    }


            //                    try
            //                    {
            //                        _searcher = new IndexSearcher(GetLuceneDirectory(), true);
            //                    }
            //                    catch (IOException ex)
            //                    {
            //                        throw new ApplicationException("Could not create an index searcher with the supplied lucene directory", ex);
            //                    }

            //                }
            //            }
            //        }
            //    }

            //}
        }
	}
}
