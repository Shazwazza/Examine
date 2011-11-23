using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Examine;
using Examine.SearchCriteria;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Analysis;


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
        public LuceneSearcher(DirectoryInfo workingFolder, Analyzer analyzer)
            : base(analyzer)
		{
            LuceneIndexFolder = new DirectoryInfo(Path.Combine(workingFolder.FullName, "Index"));
            EnsureIndex();
		}

		#endregion

		/// <summary>
		/// Used as a singleton instance
		/// </summary>
		private IndexSearcher _searcher;
		private static readonly object Locker = new object();

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
			if (config["indexSet"] == null && LuceneIndexFolder == null)
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
			else if (config["indexSet"] != null)
			{
				if (IndexSets.Instance.Sets[config["indexSet"]] == null)
					throw new ArgumentException("The indexSet specified for the LuceneExamineIndexer provider does not exist");

				IndexSetName = config["indexSet"];

				//get the folder to index
				LuceneIndexFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));
			}

		    EnsureIndex();
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
			protected set
			{
				_indexFolder = value;
                //the path is changing, close the current searcher.
			    ValidateSearcher(true);
			}
		}
		
		/// <summary>
		/// Do not access this object directly. The public property ensures that the folder state is always up to date
		/// </summary>
		private DirectoryInfo _indexFolder;

        /// <summary>
        /// Ensures the index exists at the location
        /// </summary>
        public void EnsureIndex()
        {
            IndexWriter writer = null;
            try
            {
                if (!IndexReader.IndexExists(GetLuceneDirectory()))
                {
                    //create the writer (this will overwrite old index files)
                    writer = new IndexWriter(GetLuceneDirectory(), IndexingAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                }
            }            
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                    writer = null;
                }
            }

        }

        /// <summary>
        /// A simple search mechanism to search all fields based on an index type.
        /// </summary>
        /// <remarks>
        /// This can be used to do a simple search against an index type instead of the entire index.
        /// </remarks>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <param name="indexType"></param>
        /// <returns></returns>
        public ISearchResults Search(string searchText, bool useWildcards, string indexType)
        {
            var sc = CreateSearchCriteria(indexType);

            if (useWildcards)
            {
                var wildcardSearch = new ExamineValue(Examineness.ComplexWildcard, searchText.MultipleCharacterWildcard().Value);
                sc = sc.GroupedOr(GetSearchFields(), wildcardSearch).Compile();
            }
            else
            {
                sc = sc.GroupedOr(GetSearchFields(), searchText).Compile();
            }

            return Search(sc);
        }

        /// <summary>
        /// Searches the data source using the Examine Fluent API
        /// </summary>
        /// <param name="searchParams">The fluent API search.</param>
        /// <returns></returns>
        public override ISearchResults Search(ISearchCriteria searchParams)
        {
            if (!LuceneIndexFolder.Exists)
                throw new DirectoryNotFoundException("No index found at the location specified. Ensure that an index has been created");

            return base.Search(searchParams);
        }

        /// <summary>
        /// Name of the Lucene.NET index set
        /// </summary>
        protected string IndexSetName { get; private set; }

        /// <summary>
        /// Gets the searcher for this instance
        /// </summary>
        /// <returns></returns>
        public override Searcher GetSearcher()
        {
            ValidateSearcher(false);

            //ensure scoring is turned on for sorting
            _searcher.SetDefaultFieldSortScoring(true, true);
            return _searcher;
        }
        
        /// <summary>
        /// Returns a list of fields to search on
        /// </summary>
        /// <returns></returns>
        protected override internal string[] GetSearchFields()
        {
            ValidateSearcher(false);

            var reader = _searcher.GetIndexReader();
            var fields = reader.GetFieldNames(IndexReader.FieldOption.ALL);
            //exclude the special index fields
            var searchFields = fields
                .Where(x => !x.StartsWith(LuceneIndexer.SpecialFieldPrefix))
                .ToArray();
            return searchFields;
        }

        protected virtual Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            return new SimpleFSDirectory(LuceneIndexFolder);
        }

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
        internal protected void ValidateSearcher(bool forceReopen)
        {
            if (!forceReopen)
            {
                if (_searcher == null)
                {
                    lock (Locker)
                    {
                        //double check
                        if (_searcher == null)
                        {

                            try
                            {
                                _searcher = new IndexSearcher(GetLuceneDirectory(), true);
                            }
                            catch (IOException ex)
                            {
                                throw new ApplicationException("There is no Lucene index in the folder: " + LuceneIndexFolder.FullName + " to create an IndexSearcher on", ex);
                            }
                        }
                    }
                }
                else
                {
                    if (_searcher.GetReaderStatus() != ReaderStatus.Current)
                    {
                        lock (Locker)
                        {
                            //double check, now, we need to find out if it's closed or just not current
                            switch (_searcher.GetReaderStatus())
                            {
                                case ReaderStatus.Current:
                                    break;
                                case ReaderStatus.Closed:
                                    _searcher = new IndexSearcher(GetLuceneDirectory(), true);
                                    break;
                                case ReaderStatus.NotCurrent:

                                    //yes, this is actually the way the Lucene wants you to work...
                                    //normally, i would have thought just calling Reopen() on the underlying reader would suffice... but it doesn't.
                                    //here's references: 
                                    // http://stackoverflow.com/questions/1323779/lucene-indexreader-reopen-doesnt-seem-to-work-correctly
                                    // http://gist.github.com/173978 

                                    var oldReader = _searcher.GetIndexReader();
                                    var newReader = oldReader.Reopen(true);
                                    if (newReader != oldReader)
                                    {
                                        _searcher.Close();
                                        oldReader.Close();
                                        _searcher = new IndexSearcher(newReader);
                                    }

                                    break;
                            }
                        }
                    }
                }
            }
            else
            {
                //need to close the searcher and force a re-open

                if (_searcher != null)
                {
                    lock (Locker)
                    {
                        //double check
                        if (_searcher != null)
                        {
                            try
                            {
                                _searcher.Close();
                            }
                            catch (IOException)
                            {
                                //this will happen if it's already closed ( i think )
                            }
                            finally
                            {
                                //set to null in case another call to this method has passed the first lock and is checking for null
                                _searcher = null;
                            }


                            try
                            {
                                _searcher = new IndexSearcher(GetLuceneDirectory(), true);
                            }
                            catch (IOException ex)
                            {
                                throw new ApplicationException("There is no Lucene index in the folder: " + LuceneIndexFolder.FullName + " to create an IndexSearcher on", ex);
                            }
                            
                        }
                    }
                }
            }

        }

	}
}
