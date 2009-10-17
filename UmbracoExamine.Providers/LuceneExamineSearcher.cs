using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoExamine.Core;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Analysis.Standard;
using System.IO;
using UmbracoExamine.Providers.Config;
using System.Collections;

namespace UmbracoExamine.Providers
{
    public class LuceneExamineSearcher : BaseSearchProvider
    {

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            //need to check if the index set is specified
            if (config["indexSet"] == null)
                throw new ArgumentNullException("indexSet on LuceneExamineIndexer provider has not been set in configuration and/or the IndexerData property has not been explicitly set");

            if (ExamineLuceneIndexes.Instance.Sets[config["indexSet"]] == null)
                throw new ArgumentException("The indexSet specified for the LuceneExamineIndexer provider does not exist");

            IndexSetName = config["indexSet"];
            
            //get the folder to index
            LuceneIndexFolder = ExamineLuceneIndexes.Instance.Sets[IndexSetName].IndexDirectory;
        }

        public DirectoryInfo LuceneIndexFolder { get; protected set; }

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
            return Search(new SearchCriteria(searchText, new string[] { }, new string[] { }, useWildcards, null, maxResults));
        }

        public override IEnumerable<SearchResult> Search(ISearchCriteria searchParams)
        {
            string text;

            string[] searchFields = searchParams.SearchFields.ToArray();
            IndexSearcher searcher = null;
            try
            {
                text = searchParams.Text.ToLower();

                List<SearchResult> results = new List<SearchResult>();
                //search string needs to be bigger than 2 chars
                if (string.IsNullOrEmpty(text) || text.Length < 3)
                    return results;

                // Remove all entries that are 2 letters or less, remove other invalid search chars. Replace all " " with AND 
                string queryText = PrepareSearchText(text, true, searchParams.MatchAllWords);

                if (!LuceneIndexFolder.Exists)
                    throw new DirectoryNotFoundException("No index found at the location specified. Ensure that an index has been created");

                searcher = new IndexSearcher(LuceneIndexFolder.FullName);

                //create the full query
                BooleanQuery fullQry = new BooleanQuery();             

                //add the type query
                fullQry.Add(GetIndexTypeQuery(searchParams.SearchIndexType), BooleanClause.Occur.MUST);

                foreach (var nodeType in searchParams.NodeTypeAliases)
                {
                    fullQry.Add(GetNodeTypeLookupQuery(nodeType), BooleanClause.Occur.MUST);
                }

                //add the path query if specified
                if (searchParams.ParentNodeId.HasValue)
                {
                    Query qryParent = GetParentDocQuery(searchParams.ParentNodeId.Value);
                    if (qryParent == null)
                        return results;
                    fullQry.Add(qryParent, BooleanClause.Occur.MUST);
                }

                

                //create an inner query to query our fields using both an exact match and wildcard match.
                BooleanQuery fieldQry = new BooleanQuery();

                //if there are no field specified, we'll search them all
                if (searchFields.Length == 0)
                {
                    IndexReader reader = searcher.GetIndexReader();
                    var fields = reader.GetFieldNames(IndexReader.FieldOption.ALL);
                    //exclude the special index fields
                    searchFields = fields.Cast<DictionaryEntry>()
                        .Where(x => x.Key.ToString() != LuceneExamineIndexer.IndexNodeIdFieldName || x.Key.ToString() != LuceneExamineIndexer.IndexTypeFieldName)
                        .Select(x => (string)x.Value)
                        .ToArray();                        
                }
                

                Query standardFieldQry = GetStandardFieldQuery(queryText, searchFields);
                fieldQry.Add(standardFieldQry, BooleanClause.Occur.SHOULD);
                if (searchParams.UseWildcards)
                {
                    //get the wildcard query
                    Query wildcardFieldQry = GetWildcardFieldQuery(queryText, searchFields, searchParams.MatchAllWords);
                    //change the weighting of the queries so exact match have a higher priority
                    standardFieldQry.SetBoost(2);
                    wildcardFieldQry.SetBoost((float)0.5);
                    fieldQry.Add(wildcardFieldQry, BooleanClause.Occur.SHOULD);
                }

                fullQry.Add(fieldQry, BooleanClause.Occur.MUST);

                TopDocs tDocs = searcher.Search(fullQry, (Filter)null, searchParams.MaxResults);

                results = PrepareResults(tDocs, searchFields, searcher);

                return results.ToList();
            }
            finally
            {
                if (searcher != null)
                    searcher.Close();
            }
        }

        #region Private
        /// <summary>
        /// This will create a query with wildcards to match.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private Query GetWildcardFieldQuery(string text, string[] searchFields, bool matchAllWords)
        {
            //get search terms split without booleans
            string queryText = PrepareSearchText(text, false, false);
            List<string> words = queryText.Split(" ".ToCharArray(), StringSplitOptions.None).ToList();
            
            //add * to search terms
            List<string> fixedWords = new List<string>();
            words.ForEach(x => fixedWords.Add(x + "*"));

            string wildcardQuery = PrepareSearchText(string.Join(" ", fixedWords.ToArray()), false, matchAllWords);
            //now that we have a wildcard match for each word, we'll make a query with it

            BooleanClause.Occur[] bc = new BooleanClause.Occur[searchFields.Length];
            for (int i = 0; i < bc.Length; i++)
            {
                bc[i] = BooleanClause.Occur.SHOULD;
            }

            return MultiFieldQueryParser.Parse(wildcardQuery, searchFields, bc, new StandardAnalyzer());
        }

        /// <summary>
        /// Return a standard query to query all of our fields
        /// </summary>
        /// <param name="queryText"></param>
        /// <returns></returns>
        private Query GetStandardFieldQuery(string queryText, string[] searchFields)
        {
            BooleanClause.Occur[] bc = new BooleanClause.Occur[searchFields.Length];
            for (int i = 0; i < bc.Length; i++)
            {
                bc[i] = BooleanClause.Occur.SHOULD;
            }

            return MultiFieldQueryParser.Parse(queryText, searchFields, bc, new StandardAnalyzer());
        }

        /// <summary>
        /// Return a query to query for a node type Alias
        /// </summary>
        /// <param name="nodeTypeAlias"></param>
        /// <returns></returns>
        private Query GetNodeTypeLookupQuery(string nodeTypeAlias)
        {
            // a phrase query without 'slop' is an exact match
            PhraseQuery phraseQuery = new PhraseQuery();
            string[] terms = nodeTypeAlias.Split(' ');
            foreach (string term in terms)
                phraseQuery.Add(new Term("nodeTypeAlias", term.ToLower()));
            return phraseQuery;
        }

        /// <summary>
        /// Returns a query to match the index type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Query GetIndexTypeQuery(IndexType type)
        {
            var pq = new PhraseQuery();
            pq.Add(new Term(LuceneExamineIndexer.IndexTypeFieldName, type.ToString()));
            return pq;
        }

        /// <summary>
        /// Returns a query to ensure only the children of the document type specified are returned
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        private Query GetParentDocQuery(int nodeId)
        {
            umbraco.cms.businesslogic.web.Document doc = new umbraco.cms.businesslogic.web.Document(nodeId);
            if (doc == null)
                return null;

            List<string> path = doc.Path.Split(',').ToList();
            List<string> searchPath = new List<string>();
            int idIndex = path.IndexOf(nodeId.ToString());
            for (int i = 0; i <= idIndex; i++)
                searchPath.Add(path[i]);

            //need to remove the leading "-" as Lucene will not search on this for whatever reason.
            string pathQuery = (string.Join(",", searchPath.ToArray()) + ",*").Replace("-", "");
            return new WildcardQuery(new Term("path", pathQuery));
        }

        /// <summary>
        /// Removes spaces, small strings, and invalid characters
        /// </summary>
        /// <param name="text">the text to prepare for search</param>
        /// <param name="removeWildcards">whether or not to remove wildcard chars</param>
        /// <param name="addBooleans">whether or not to add boolean "AND" logic between words. If false, words are returned with spaces.</param>
        /// <returns></returns>
        private string PrepareSearchText(string text, bool removeWildcards, bool addBooleans)
        {
            if (text.Length < 3)
                return "";

            string charsToRemove = "~!@#$%^&()_+`-={}|[]\\:\";'<>,./";

            if (removeWildcards)
                charsToRemove = charsToRemove.Replace("*", "").Replace("?", "");

            List<string> words = new List<string>();

            // Remove all spaces and strings <= 2 chars
            words = text.Trim()
                        .Split(' ')
                        .Select(x => x.ToString())
                        .Where(x => x.Length > 2).ToList();

            // Remove all other invalid chars
            for (int i = 0; i < words.Count(); i++)
                foreach (char c in charsToRemove)
                    words[i] = words[i].Replace(c.ToString(), "");

            if (addBooleans)
            {
                // Create new text
                string queryText = "";
                words.ForEach(x => queryText += " AND " + x.ToString());

                return queryText.Remove(0, 5); // remove first " AND "
            }
            else
            {
                return string.Join(" ", words.ToArray());
            }

        }

        /// <summary>
        /// Creates a list of dictionary's from the hits object and returns a list of SearchResult.
        /// This also removes duplicates.
        /// </summary>
        /// <param name="hits"></param>
        /// <param name="searchFields"></param>
        /// <returns></returns>
        private List<SearchResult> PrepareResults(TopDocs tDocs, string[] searchFields, IndexSearcher searcher)
        {
            List<SearchResult> results = new List<SearchResult>();

            for (int i = 0; i < tDocs.scoreDocs.Length; i++)
            {
                Document doc = searcher.Doc(tDocs.scoreDocs[i].doc);
                Dictionary<string, string> fields = new Dictionary<string, string>();

                foreach (Field f in doc.Fields())
                {
                    if (searchFields.Contains(f.Name()))
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
