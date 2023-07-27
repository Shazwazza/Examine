using System;
using Lucene.Net.Analysis;
using Lucene.Net.Search;
using Examine.Lucene.Search;
using Examine.Search;
using Lucene.Net.Facet;

namespace Examine.Lucene.Providers
{
    ///<summary>
    /// Simple abstract class containing basic properties for Lucene searchers
    ///</summary>
    public abstract class BaseLuceneSearcher : BaseSearchProvider, IDisposable
    {
        private readonly FacetsConfig _facetsConfig;

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="name"></param>
        /// <param name="analyzer"></param>
        /// <param name="facetsConfig"></param>
        protected BaseLuceneSearcher(string name, Analyzer analyzer, FacetsConfig facetsConfig)
            : base(name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            LuceneAnalyzer = analyzer;
            _facetsConfig = facetsConfig;
        }

        /// <summary>
        /// The analyzer to use for query parsing, by default, this is set to StandardAnalyzer
        /// </summary>
        public Analyzer LuceneAnalyzer { get; }

        /// <summary>
        /// Gets the seach context
        /// </summary>
        /// <returns></returns>
        public abstract ISearchContext GetSearchContext();

        /// <inheritdoc />
		public override IQuery CreateQuery(string? category = null, BooleanOperation defaultOperation = BooleanOperation.And)
            => CreateQuery(category, defaultOperation, LuceneAnalyzer, new LuceneSearchOptions());

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="category">The type of data in the index.</param>
        /// <param name="defaultOperation">The default operation.</param>
        /// <param name="luceneAnalyzer"></param>
        /// <param name="searchOptions"></param>
        /// <returns></returns>
        public IQuery CreateQuery(string? category, BooleanOperation defaultOperation, Analyzer luceneAnalyzer, LuceneSearchOptions searchOptions)
        {
            if (luceneAnalyzer == null)
                throw new ArgumentNullException(nameof(luceneAnalyzer));

            return new LuceneSearchQuery(GetSearchContext(), category, luceneAnalyzer, searchOptions, defaultOperation, _facetsConfig);
        }

        /// <inheritdoc />
        public override ISearchResults Search(string searchText, QueryOptions? options = null)
        {
            var sc = CreateQuery().ManagedQuery(searchText);
            return sc.Execute(options);
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {

        }

        ///// <summary>
        ///// This is NOT used! however I'm leaving this here as example code
        ///// 
        ///// This is used to recursively set any query type that supports <see cref="MultiTermQuery.RewriteMethod"/> parameters for rewriting
        ///// before the search executes.
        ///// </summary>
        ///// <param name="query"></param>
        ///// <remarks>
        ///// Normally this is taken care of with QueryParser.SetMultiTermRewriteMethod(MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE) however
        ///// that would need to be set eagerly before any query parsing takes place but if we want to do it lazily here's how.
        ///// So we need to manually update any query within the outer boolean query with the correct rewrite method, then the underlying LuceneSearcher will call rewrite
        ///// to update everything.
        ///// 
        ///// see https://github.com/Shazwazza/Examine/pull/89
        ///// see https://lists.gt.net/lucene/java-user/92194
        ///// 
        ///// </remarks>
        //private void SetScoringBooleanQueryRewriteMethod(Query query)
        //{


        //    if (query is MultiTermQuery mtq)
        //    {
        //        try
        //        {
        //            mtq.MultiTermRewriteMethod = ErrorCheckingScoringBooleanQueryRewriteInstance;
        //        }
        //        catch (NotSupportedException)
        //        {
        //            //swallow this, some implementations of MultiTermQuery don't support this like FuzzyQuery
        //        }
        //    }
        //    if (query is BooleanQuery bq)
        //    {
        //        foreach (BooleanClause clause in bq.Clauses)
        //        {
        //            var q = clause.Query;
        //            //recurse
        //            SetScoringBooleanQueryRewriteMethod(q);
        //        }
        //    }
        //}

        //private static MultiTermQuery.RewriteMethod s_errorCheckingScoringBooleanQueryRewriteInstance;
        //public static MultiTermQuery.RewriteMethod ErrorCheckingScoringBooleanQueryRewriteInstance => s_errorCheckingScoringBooleanQueryRewriteInstance ?? (s_errorCheckingScoringBooleanQueryRewriteInstance = new ErrorCheckingScoringBooleanQueryRewrite());

    }
}
