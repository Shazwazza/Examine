using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Text;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
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
    public abstract class BaseLuceneSearcher : BaseSearchProvider<ILuceneSearchResults, LuceneSearchResult, LuceneSearchCriteria>, ILuceneSearcher
    {

        #region Constructors

        /// <summary>
        /// Constructor used for provider instantiation
        /// </summary>
        protected BaseLuceneSearcher()
        {
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="analyzer"></param>
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
            
            get;
            
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


        /// <summary>
        /// Gets the searcher for this instance
        /// </summary>
        /// <returns></returns>
        public abstract Searcher GetSearcher();

        
        /// <summary>
        /// Gets the CriteriaContext
        /// </summary>
        /// <returns></returns>
        public abstract ICriteriaContext GetCriteriaContext();        
    

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
        [Obsolete("Use the Find method instead for strongly typed search results")]
        public override ISearchResults Search(string searchText, bool useWildcards)
        {
            var result = Find(searchText, useWildcards);
            return new SearchResultsProxy<LuceneSearchResult>(result);
        }

        internal ILuceneSearchResults TextSearchAllFields(string searchText, bool useWildcards, ISearchCriteria sc)
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

            return Find(sc);
        }

        /// <summary>
        /// Performs a search with a standard result
        /// </summary>        
        [Obsolete("Use the Find method instead for strongly typed search results")]
        public override ISearchResults Search(ISearchCriteria searchParams)
        {
            var pagesResults = Find(searchParams);
            return new SearchResultsProxy<LuceneSearchResult>(pagesResults);
        }

        [Obsolete("Use the Find method with MaxCount instead for strongly typed search results")]
        public override ISearchResults Search(ISearchCriteria searchParams, int maxResults)
        {
            var pagesResults = Find(searchParams.MaxCount(maxResults));
            return new SearchResultsProxy<LuceneSearchResult>(pagesResults);
        }

        public override ILuceneSearchResults Find(string searchText, bool useWildcards)
        {
            var sc = this.CreateCriteria();
            return TextSearchAllFields(searchText, useWildcards, sc);
        }
        /// <summary>
        /// Performs a search with a typed result
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        public override ILuceneSearchResults Find(ISearchCriteria searchParams)
        {
            Enforcer.ArgumentNotNull(searchParams, "searchParams");

            var luceneParams = searchParams as LuceneSearchCriteria;
            if (luceneParams == null)
                throw new ArgumentException("Provided ISearchCriteria dos not match the allowed ISearchCriteria. Ensure you only use an ISearchCriteria created from the current SearcherProvider");

            luceneParams.CriteriaContext = GetCriteriaContext();

            luceneParams.SearchOptions.MaxCount = searchParams.MaxResults;

            var pagesResults = new LuceneSearchResults(luceneParams.Query, luceneParams.SortFields, luceneParams.CriteriaContext, luceneParams.SearchOptions);

            foreach (var type in luceneParams.CriteriaContext.ValueTypes)
            {
                var hl = type.GetHighlighter(luceneParams.Query, luceneParams.CriteriaContext.Searcher, luceneParams.CriteriaContext.FacetsLoader);
                if (hl != null)
                {
                    List<Func<LuceneSearchResult, string>> highlights;
                    if (!pagesResults.Highlighters.TryGetValue(type.FieldName, out highlights))
                    {
                        pagesResults.Highlighters.Add(type.FieldName, highlights = new List<Func<LuceneSearchResult, string>>());
                    }

                    highlights.Add(res => hl.Highlight(res.DocId));
                }
            }


            //foreach (var fq in luceneParams.CriteriaContext.FieldQueries)
            //{
            //    var hl = fq.Key.GetHighlighter(luceneParams.Query, luceneParams.CriteriaContext.Searcher, luceneParams.CriteriaContext.FacetsLoader);
            //    if (hl != null)
            //    {                    
            //        foreach (var r in pagesResults)
            //        {
            //            List<Func<string>> highlights;
            //            if (!r.Highlights.TryGetValue(fq.Key.FieldName, out highlights))
            //            {
            //                r.Highlights.Add(fq.Key.FieldName, highlights = new List<Func<string>>());
            //            }

            //            string value = null;
            //            highlights.Add(() => value ?? (value = hl.Highlight(r.DocId)));
            //        }
            //    }
            //}

            return pagesResults;
        }     

        /// <summary>
        /// Creates search criteria that defaults to IndexType.Any and BooleanOperation.And
        /// </summary>
        /// <returns></returns>        
        [Obsolete("Use the CreateCriteria method instead for strongly typed search criteria")]
        public override ISearchCriteria CreateSearchCriteria()
        {
            return CreateSearchCriteria(string.Empty, BooleanOperation.And);
        }

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        [Obsolete("Use the CreateCriteria method instead for strongly typed search criteria")]
        public override ISearchCriteria CreateSearchCriteria(string type)
        {
            return CreateSearchCriteria(type, BooleanOperation.And);
        }

        [Obsolete("Use the CreateCriteria method instead for strongly typed search criteria")]
        public override ISearchCriteria CreateSearchCriteria(BooleanOperation defaultOperation)
        {
            return CreateSearchCriteria(string.Empty, defaultOperation);
        }

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <param name="defaultOperation">The default operation.</param>
        /// <returns>A blank SearchCriteria</returns>
        [Obsolete("Use the CreateCriteria method instead for strongly typed search criteria")]
        public override ISearchCriteria CreateSearchCriteria(string type, BooleanOperation defaultOperation)
        {
            return CreateCriteria(type, defaultOperation);
        }    

        /// <summary>
        /// Creates Criteria specific to lucene searches
        /// </summary>
        /// <param name="type"></param>
        /// <param name="defaultOperation"></param>
        /// <returns></returns>
        public override LuceneSearchCriteria CreateCriteria(string type = null, BooleanOperation defaultOperation = BooleanOperation.And)
        {
            if (type == null) type = string.Empty;

            return new LuceneSearchCriteria(this, type, IndexingAnalyzer, GetSearchFields(), EnableLeadingWildcards, defaultOperation);
        }


    }
}