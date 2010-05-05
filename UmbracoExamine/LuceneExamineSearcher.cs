using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Examine;
using Examine.Providers;
using Examine.SearchCriteria;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using UmbracoExamine.Config;
using UmbracoExamine.SearchCriteria;

namespace UmbracoExamine
{
    /// <summary>
    /// An Examine searcher which uses Lucene.Net as the 
    /// </summary>
    public class LuceneExamineSearcher : BaseSearchProvider
    {
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

            //need to check if the index set is specified
            if (config["indexSet"] == null)
                throw new ArgumentNullException("indexSet on LuceneExamineIndexer provider has not been set in configuration and/or the IndexerData property has not been explicitly set");

            if (ExamineLuceneIndexes.Instance.Sets[config["indexSet"]] == null)
                throw new ArgumentException("The indexSet specified for the LuceneExamineIndexer provider does not exist");

            IndexSetName = config["indexSet"];

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

            //get the folder to index
            LuceneIndexFolder = new DirectoryInfo(Path.Combine(ExamineLuceneIndexes.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));
        }

        private static IndexSearcher Searcher;

        /// <summary>
        /// Directory where the Lucene.NET Index resides
        /// </summary>
        public DirectoryInfo LuceneIndexFolder { get; protected set; }

        /// <summary>
        /// The analyzer to use when searching content, by default, this is set to StandardAnalyzer
        /// </summary>
        public Analyzer IndexingAnalyzer { get; set; }

        /// <summary>
        /// Name of the Lucene.NET index set
        /// </summary>
        protected string IndexSetName { get; set; }

        /// <summary>
        /// Simple search method which defaults to searching content nodes
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <returns></returns>
        public override ISearchResults Search(string searchText, bool useWildcards)
        {
            var sc = this.CreateSearchCriteria(IndexType.Content);

            if (useWildcards)
            {
                sc = sc.GroupedOr(GetSearchFields(), searchText.MultipleCharacterWildcard().Value).Compile();
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
            ValidateSearcher(false);

            Enforcer.ArgumentNotNull(searchParams, "searchParams");

            var luceneParams = searchParams as LuceneSearchCriteria;
            if (luceneParams == null)
                throw new ArgumentException("Provided ISearchCriteria dos not match the allowed ISearchCriteria. Ensure you only use an ISearchCriteria created from the current SearcherProvider");

            if (!LuceneIndexFolder.Exists)
                throw new DirectoryNotFoundException("No index found at the location specified. Ensure that an index has been created");

            var pagesResults = new SearchResults(luceneParams.query, luceneParams.sortFields, Searcher);
            return pagesResults;
        }

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <returns>A blank SearchCriteria</returns>
        public override ISearchCriteria CreateSearchCriteria(IndexType type)
        {
            return this.CreateSearchCriteria(type, BooleanOperation.And);
        }

        /// <summary>
        /// Creates an instance of SearchCriteria for the provider
        /// </summary>
        /// <param name="type">The type of data in the index.</param>
        /// <param name="defaultOperation">The default operation.</param>
        /// <returns>A blank SearchCriteria</returns>
        public override ISearchCriteria CreateSearchCriteria(IndexType type, BooleanOperation defaultOperation)
        {
            return new LuceneSearchCriteria(type, this.IndexingAnalyzer, this.GetSearchFields(), defaultOperation);
        }

        /// <summary>
        /// Gets the searcher for this instance
        /// </summary>
        /// <returns></returns>
        public IndexSearcher GetSearcher()
        {
            ValidateSearcher(false);
            return Searcher;
        }

        #region Private

        private string[] GetSearchFields()
        {
            var searcher = GetSearcher();
            return GetSearchFields(searcher.GetIndexReader());
        }

        private static string[] GetSearchFields(IndexReader reader)
        {
            var fields = reader.GetFieldNames(IndexReader.FieldOption.ALL);
            //exclude the special index fields
            var searchFields = fields
                .Where(x => x != LuceneExamineIndexer.IndexNodeIdFieldName && x != LuceneExamineIndexer.IndexTypeFieldName)
                .ToArray();
            return searchFields;
        }

        internal void ValidateSearcher(bool forceReopen)
        {
            if (!forceReopen)
            {
                if (Searcher == null)
                {
                    Searcher = new IndexSearcher(new SimpleFSDirectory(LuceneIndexFolder), true);
                }
                else if (!Searcher.GetIndexReader().IsCurrent())
                {
                    Searcher.GetIndexReader().Reopen();
                }
            }
            else
            {
                Searcher = new IndexSearcher(new SimpleFSDirectory(LuceneIndexFolder), true);
            }
        }

        #endregion
    }
}
