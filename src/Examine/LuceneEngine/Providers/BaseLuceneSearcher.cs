using System;
using System.Collections.Specialized;
using System.IO;
using System.Security;
using System.Text;
using Examine.LuceneEngine.SearchCriteria;
using Examine.Providers;
using Examine.SearchCriteria;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using System.Linq;

namespace Examine.LuceneEngine.Providers
{
    ///<summary>
    /// Simple abstract class containing basic properties for Lucene searchers
    ///</summary>
    public abstract class BaseLuceneSearcher : BaseSearchProvider
    {

        #region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		protected BaseLuceneSearcher()
		{
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="analyzer"></param>
		[SecuritySafeCritical]
        protected BaseLuceneSearcher(Analyzer analyzer)
		{           
            IndexingAnalyzer = analyzer;
		}

		#endregion

        /// <summary>
        /// Used to specify if leading wildcards are allowed. WARNING SLOWS PERFORMANCE WHEN ENABLED!
        /// </summary>
        public bool EnableLeadingWildcards { get; set; }

	    /// <summary>
	    /// The analyzer to use when searching content, by default, this is set to StandardAnalyzer
	    /// </summary>
	    public Analyzer IndexingAnalyzer
	    {
			[SecuritySafeCritical]
		    get;
			[SecuritySafeCritical]
			protected internal set;
	    }

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

            if (config["analyzer"] != null)
            {
                //this should be a fully qualified type
                var analyzerType = TypeHelper.FindType(config["analyzer"]);
                IndexingAnalyzer = (Analyzer)Activator.CreateInstance(analyzerType);
            }
            else
            {
                IndexingAnalyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
            }

            if (config["enableLeadingWildcard"] != null)
            {
                EnableLeadingWildcards = Boolean.Parse(config["enableLeadingWildcard"]);
            }

        }

        protected internal abstract string[] GetSearchFields();
        
        ///<summary>
        /// returns the underlying Lucene searcher
        ///</summary>
        ///<returns></returns>
		[SecuritySafeCritical]
        public abstract Searcher GetSearcher();

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <param name="defaultOperation">The default operation.</param>
        /// <returns>A blank SearchCriteria</returns>
		[SecuritySafeCritical]
		public override ISearchCriteria CreateSearchCriteria(string type, BooleanOperation defaultOperation)
        {
            //TODO: It would be nice to be able to pass in the existing field definitions here so 
            // we'd know what fields have sorting enabled so we don't have to use the special sorting syntax for field types

            return new LuceneSearchCriteria(type, IndexingAnalyzer, GetSearchFields(), EnableLeadingWildcards, defaultOperation);
        }

        /// <summary>
        /// Simple search method which defaults to searching content nodes
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <returns></returns>
        /// <remarks>
        /// This will search every field for any words matching in search text. Each word in the search text will be encapsulated 
        /// in a wild card search too.
        /// </remarks>
        public override ISearchResults Search(string searchText, bool useWildcards)
        {
            var sc = this.CreateSearchCriteria();
            return TextSearchAllFields(searchText, useWildcards, sc);
        }

        internal ISearchResults TextSearchAllFields(string searchText, bool useWildcards, ISearchCriteria sc)
        {
            var splitSearch = searchText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (useWildcards)
            {
                sc = sc.GroupedOr(GetSearchFields(),
                    splitSearch.Select(x =>
                        new ExamineValue(Examineness.ComplexWildcard, x.MultipleCharacterWildcard().Value)).Cast<IExamineValue>().ToArray()
                    ).Compile();                
            }
            else
            {
                sc = sc.GroupedOr(GetSearchFields(), splitSearch).Compile();
            }

            return Search(sc);
        }

        /// <summary>
        /// Performs a search
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        public override ISearchResults Search(ISearchCriteria searchParams)
        {
            return Search(searchParams, 0);
        }

        /// <summary>
        /// Performs a search with a maximum number of results
        /// </summary>        
        [SecuritySafeCritical]
        public override ISearchResults Search(ISearchCriteria searchParams, int maxResults)
        {
            Enforcer.ArgumentNotNull(searchParams, "searchParams");

            var luceneParams = searchParams as LuceneSearchCriteria;
            if (luceneParams == null)
                throw new ArgumentException("Provided ISearchCriteria dos not match the allowed ISearchCriteria. Ensure you only use an ISearchCriteria created from the current SearcherProvider");

            var searcher = GetSearcher();
            if (searcher == null) return new EmptySearchResults();

            var pagesResults = new SearchResults(luceneParams.Query, luceneParams.SortFields, searcher, maxResults,  luceneParams.Selector);

            return pagesResults;
        }

        /// <summary>
        /// This is NOT used! however I'm leaving this here as example code
        /// 
        /// This is used to recursively set any query type that supports <see cref="MultiTermQuery.RewriteMethod"/> parameters for rewriting
        /// before the search executes.
        /// </summary>
        /// <param name="query"></param>
        /// <remarks>
        /// Normally this is taken care of with QueryParser.SetMultiTermRewriteMethod(MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE) however
        /// that would need to be set eagerly before any query parsing takes place but if we want to do it lazily here's how.
        /// So we need to manually update any query within the outer boolean query with the correct rewrite method, then the underlying LuceneSearcher will call rewrite
        /// to update everything.
        /// 
        /// see https://github.com/Shazwazza/Examine/pull/89
        /// see https://lists.gt.net/lucene/java-user/92194
        /// 
        /// </remarks>
        [SecuritySafeCritical]
        protected void SetScoringBooleanQueryRewriteMethod(Query query)
        {
            

            if (query is MultiTermQuery mtq)
            {
                try
                {
                    mtq.SetRewriteMethod(ErrorCheckingScoringBooleanQueryRewriteInstance);
                }
                catch (NotSupportedException)
                {
                    //swallow this, some implementations of MultiTermQuery don't support this like FuzzyQuery
                }
            }
            if (query is BooleanQuery bq)
            {
                foreach (BooleanClause clause in bq.Clauses())
                {
                    var q = clause.GetQuery();
                    //recurse
                    SetScoringBooleanQueryRewriteMethod(q);
                }
            }
        }
        
        //do not try to set this here as a readonly field - the stupid medium trust transparency rules will throw up all over the place
        private static MultiTermQuery.RewriteMethod _errorCheckingScoringBooleanQueryRewriteInstance;

        public static MultiTermQuery.RewriteMethod ErrorCheckingScoringBooleanQueryRewriteInstance
        {
            [SecuritySafeCritical]
            get { return _errorCheckingScoringBooleanQueryRewriteInstance ?? (_errorCheckingScoringBooleanQueryRewriteInstance = new ErrorCheckingScoringBooleanQueryRewrite()); }
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
        public override ISearchResults Search(string searchText, bool useWildcards, string indexType)
        {
            var sc = CreateSearchCriteria(indexType);
            return TextSearchAllFields(searchText, useWildcards, sc);
        }

        /// <summary>
        /// Creates search criteria that defaults to IndexType.Any and BooleanOperation.And
        /// </summary>
        /// <returns></returns>
        [SecuritySafeCritical]
        public override ISearchCriteria CreateSearchCriteria()
        {
            return CreateSearchCriteria(string.Empty, BooleanOperation.And);
        }

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        public override ISearchCriteria CreateSearchCriteria(string type)
        {
            return CreateSearchCriteria(type, BooleanOperation.And);
        }

		[SecuritySafeCritical]
        public override ISearchCriteria CreateSearchCriteria(BooleanOperation defaultOperation)
        {
            return CreateSearchCriteria(string.Empty, defaultOperation);
        }


    }
}