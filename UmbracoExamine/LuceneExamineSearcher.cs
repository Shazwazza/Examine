using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Examine;
using Examine.Providers;
using Examine.SearchCriteria;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using UmbracoExamine.Config;
using UmbracoExamine.SearchCriteria;

namespace UmbracoExamine
{
    public class LuceneExamineSearcher : BaseSearchProvider
    {
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
        /// <param name="maxResults"></param>
        /// <param name="useWildcards"></param>
        /// <returns></returns>
        public override IEnumerable<SearchResult> Search(string searchText, int maxResults, bool useWildcards)
        {
            var sc = this.CreateSearchCriteria(maxResults, IndexType.Content);

            if (useWildcards)
            {
                sc = sc.MultipleFields(GetSearchFields(), searchText.MultipleCharacterWildcard()).Compile();
            }
            else
            {
                sc = sc.MultipleFields(GetSearchFields(), searchText).Compile();
            }

            return Search(sc);
        }

        public override IEnumerable<SearchResult> Search(ISearchCriteria searchParams)
        {
            IndexSearcher searcher = null;

            try
            {
                var luceneParams = searchParams as LuceneSearchCriteria;
                if (luceneParams == null)
                    throw new ArgumentException("Provided ISearchCriteria dos not match the allowed ISearchCriteria. Ensure you only use an ISearchCriteria created from the current SearcherProvider");

                if (!LuceneIndexFolder.Exists)
                    throw new DirectoryNotFoundException("No index found at the location specified. Ensure that an index has been created");

                searcher = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(LuceneIndexFolder), true);

                TopDocs tDocs = searcher.Search(luceneParams.query, (Filter)null, searchParams.MaxResults);

                IndexReader reader = searcher.GetIndexReader();
                var searchFields = GetSearchFields(reader);

                var results = PrepareResults(tDocs, searchFields, searcher);

                if (luceneParams.IncludeHitCount)
                {
                    //TODO: Work out how to do this properly!!!
                    var hits = searcher.Search(luceneParams.query, null, luceneParams.MaxResults);
                    luceneParams.TotalHits = hits.totalHits;
                }

                return results.ToList();
            }
            finally
            {
                if (searcher != null)
                {
                    searcher.Close();
                }
            }
        }

        public override ISearchCriteria CreateSearchCriteria(int maxResults, IndexType type)
        {
            return new LuceneSearchCriteria(maxResults, type);
        }

        #region Private

        private string[] GetSearchFields()
        {
            var searcher = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(LuceneIndexFolder), true);
            try
            {
                return GetSearchFields(searcher.GetIndexReader());
            }
            finally
            {
                searcher.Close();
            }
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

        /// <summary>
        /// Creates a list of dictionary's from the hits object and returns a list of SearchResult.
        /// This also removes duplicates.
        /// </summary>
        /// <param name="tDocs">The top docs.</param>
        /// <param name="searchFields">The search fields.</param>
        /// <param name="searcher">The searcher.</param>
        /// <returns></returns>
        private List<SearchResult> PrepareResults(TopDocs tDocs, string[] searchFields, IndexSearcher searcher)
        {
            List<SearchResult> results = new List<SearchResult>();

            for (int i = 0; i < tDocs.scoreDocs.Length; i++)
            {
                Document doc = searcher.Doc(tDocs.scoreDocs[i].doc);
                Dictionary<string, string> fields = new Dictionary<string, string>();

                foreach (Field f in doc.GetFields())
                {
                    //if (searchFields.Contains(f.Name()))
                    fields.Add(f.Name(), f.StringValue());
                }

                results.Add(new SearchResult()
                {
                    Score = tDocs.scoreDocs[i].score,
                    Id = int.Parse(fields["id"]), //if the id field isn't indexed in the config, an error will occur!
                    Fields = fields
                });
            }

            //return the distinct results ordered by the highest score descending.
            return (from r in results.Distinct().ToList()
                    orderby r.Score descending
                    select r).ToList();
        }
        #endregion
    }
}
