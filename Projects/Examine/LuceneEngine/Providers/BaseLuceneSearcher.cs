using System;
using System.Collections.Specialized;
using System.IO;
using System.Security;
using System.Text;
using Examine.LuceneEngine.Faceting;
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
        /// Configuration for how to extract facets
        /// </summary>
        public FacetConfiguration FacetConfiguration { get; set; }

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
                var analyzerType = Type.GetType(config["analyzer"]);
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


        [SecuritySafeCritical]
        public abstract ISearcherContext GetSearcherContext();

        ///<summary>
        /// returns the underlying Lucene searcher
        ///</summary>
        ///<returns></returns>
        [SecuritySafeCritical]
        public Searcher GetSearcher()
        {
            return GetSearcherContext().LuceneSearcher;
        }

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <param name="defaultOperation">The default operation.</param>
        /// <returns>A blank SearchCriteria</returns>
        [SecuritySafeCritical]
        public override ISearchCriteria CreateSearchCriteria(string type, BooleanOperation defaultOperation)
        {
            return new LuceneSearchCriteria(this, type, IndexingAnalyzer, GetSearchFields(), EnableLeadingWildcards, defaultOperation);
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
            var searcher = GetSearcherContext();

            Enforcer.ArgumentNotNull(searchParams, "searchParams");

            var luceneParams = searchParams as LuceneSearchCriteria;
            if (luceneParams == null)
                throw new ArgumentException("Provided ISearchCriteria dos not match the allowed ISearchCriteria. Ensure you only use an ISearchCriteria created from the current SearcherProvider");

            luceneParams.SearcherContext = GetSearcherContext();

            var pagesResults = new SearchResults(luceneParams.Query, luceneParams.SortFields, searcher, luceneParams.SearchOptions);
            return pagesResults;
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