using System;
using System.Collections.Specialized;
using System.IO;
using System.Security;
using System.Text;
using Examine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using System.Linq;
using Examine.LuceneEngine.Search;
using Examine.Search;
using Version = Lucene.Net.Util.Version;

namespace Examine.LuceneEngine.Providers
{
    ///<summary>
    /// Simple abstract class containing basic properties for Lucene searchers
    ///</summary>
    public abstract class BaseLuceneSearcher : BaseSearchProvider
    {
        private readonly string _name;

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
        /// <param name="name"></param>
        /// <param name="analyzer"></param>
        protected BaseLuceneSearcher(string name, Analyzer analyzer)
		{
		    if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
		    LuceneAnalyzer = analyzer;
		    _name = name;
        }

		#endregion
        
	    /// <summary>
	    /// The analyzer to use when searching content, by default, this is set to StandardAnalyzer
	    /// </summary>
	    public Analyzer LuceneAnalyzer
	    {
		    get;
			private set;
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
		
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            if (config["analyzer"] != null)
            {
                //this should be a fully qualified type
                var analyzerType = TypeHelper.FindType(config["analyzer"]);
                if (typeof(StandardAnalyzer).IsAssignableFrom(analyzerType))
                    LuceneAnalyzer = (Analyzer)Activator.CreateInstance(analyzerType, Version.LUCENE_30);
                else
                    LuceneAnalyzer = (Analyzer)Activator.CreateInstance(analyzerType);
            }
            else
            {
                LuceneAnalyzer = new CultureInvariantStandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            }
        }

        public override string Name => _name ?? base.Name;

        /// <summary>
        /// Returns all field names that exist in the index
        /// </summary>
        /// <returns></returns>
        public abstract string[] GetAllIndexedFields();
        
        ///<summary>
        /// returns the underlying Lucene searcher
        ///</summary>
        ///<returns></returns>
		
        public abstract Searcher GetLuceneSearcher();

        public abstract ISearchContext GetSearchContext();

        /// <inheritdoc />
		public override IQuery CreateCriteria(string type = null, BooleanOperation defaultOperation = BooleanOperation.And)
        {
            return CreateCriteria(type, defaultOperation, LuceneAnalyzer, new LuceneSearchOptions());
        }

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <param name="defaultOperation">The default operation.</param>
        /// <param name="luceneAnalyzer"></param>
        /// <param name="searchOptions"></param>
        /// <returns></returns>
        public IQuery CreateCriteria(string type, BooleanOperation defaultOperation, Analyzer luceneAnalyzer, LuceneSearchOptions searchOptions)
        {
            if (luceneAnalyzer == null) throw new ArgumentNullException(nameof(luceneAnalyzer));

            return new LuceneSearchQuery(GetSearchContext(), type, luceneAnalyzer, GetAllIndexedFields(), searchOptions, defaultOperation);
        }

        /// <inheritdoc />
        public override ISearchResults Search(string searchText, int maxResults = 500)
        {
            var sc = CreateCriteria().ManagedQuery(searchText);
            return sc.Execute(maxResults);
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
        
        private void SetScoringBooleanQueryRewriteMethod(Query query)
        {
            

            if (query is MultiTermQuery mtq)
            {
                try
                {
                    mtq.RewriteMethod = ErrorCheckingScoringBooleanQueryRewriteInstance;
                }
                catch (NotSupportedException)
                {
                    //swallow this, some implementations of MultiTermQuery don't support this like FuzzyQuery
                }
            }
            if (query is BooleanQuery bq)
            {
                foreach (BooleanClause clause in bq.Clauses)
                {
                    var q = clause.Query;
                    //recurse
                    SetScoringBooleanQueryRewriteMethod(q);
                }
            }
        }
        
        //do not try to set this here as a readonly field - the stupid medium trust transparency rules will throw up all over the place
        private static RewriteMethod _errorCheckingScoringBooleanQueryRewriteInstance;

        public static RewriteMethod ErrorCheckingScoringBooleanQueryRewriteInstance => _errorCheckingScoringBooleanQueryRewriteInstance ?? (_errorCheckingScoringBooleanQueryRewriteInstance = new ErrorCheckingScoringBooleanQueryRewrite());
        
    }
}