using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using Examine;
using Examine.LuceneEngine.Cru;
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


namespace Examine.LuceneEngine.Providers
{
    
    ///<summary>
	/// Standard object used to search a Lucene index
	///</summary>
    public class LuceneSearcher : BaseLuceneSearcher
	{
		#region Constructors

        /// <summary>
        /// Constructor used for provider instantiation
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]        
        public LuceneSearcher()
		{
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="workingFolder"></param>
        /// <param name="analyzer"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
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
		public LuceneSearcher(Lucene.Net.Store.Directory luceneDirectory, Analyzer analyzer)
			: base(analyzer)
		{
			LuceneIndexFolder = null;            
			_luceneDirectory = luceneDirectory;
		}

		#endregion
        
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
			else if (config["indexSet"] != null && _luceneDirectory == null)
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
		/// Do not access this object directly. The public property ensures that the folder state is always up to date
		/// </summary>
		private DirectoryInfo _indexFolder;

        /// <summary>
        /// Ensures the index exists exists
        /// </summary>
		[Obsolete("This does not do anything and is no longer used")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void EnsureIndex()
        {
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
        public new ILuceneSearchResults Search(string searchText, bool useWildcards, string indexType)
        {
            var sc = CreateCriteria(indexType);
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
        public override Searcher GetSearcher()
        {
            ValidateSearcher();

            var token = _searcherContext.GetSearcher();
            DisposableCollector.Track(token);

            //ensure scoring is turned on for sorting
            token.Searcher.SetDefaultFieldSortScoring(true, true);            
            return token.Searcher;
        }

        /// <summary>
        /// Returns the searcher's criteria context
        /// </summary>
        /// <returns></returns>
        public override ICriteriaContext GetCriteriaContext()
        {
            ValidateSearcher();

            return new FacetsLoaderCriteriaContext((IndexSearcher) GetSearcher(), _searcherContext);
        }


        /// <summary>
        /// Returns a list of fields to search on
        /// </summary>
        /// <returns></returns>
        protected internal override string[] GetSearchFields()
        {
            ValidateSearcher();

            using (var s = _searcherContext.GetSearcher())
            {
                var reader = s.Searcher.IndexReader;
                var fields = reader.GetFieldNames(IndexReader.FieldOption.ALL);
                //exclude the special index fields
                var searchFields = fields
                    .Where(x => !x.StartsWith(LuceneIndexer.SpecialFieldPrefix))
                    .ToArray();
                return searchFields;
            }
        }

		
        /// <summary>
        /// Returns the lucene directory
        /// </summary>
        /// <returns></returns>
        protected virtual Lucene.Net.Store.Directory GetLuceneDirectory()
        {
			if (_luceneDirectory == null)
			{
				_luceneDirectory = new SimpleFSDirectory(LuceneIndexFolder);
			}
			return _luceneDirectory;
        }

        private SearcherContext _searcherContext;        

        /// <summary>
        /// This checks if the singleton IndexSearcher is initialized and up to date.
        /// </summary>
        private void ValidateSearcher()
        {
            if (_searcherContext == null)
            {
                _searcherContext = SearcherContextCollection.Instance.GetContext(GetLuceneDirectory());

                if (_searcherContext == null)
                {
                    throw new NotSupportedException("No indexer is defined for the directory");
                }
            }

            if (DefaultExamineSession.RequireImmediateConsistency)
            {
               DefaultExamineSession.WaitForChanges(_searcherContext.Manager);
            }

        }
	}
}
