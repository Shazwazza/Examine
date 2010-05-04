using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System.Collections;
using Lucene.Net.Documents;

namespace UmbracoExamine
{
    public class SearchResults : ISearchResults
    {
        private Searcher searcher;
        private AllHitsCollector collector;

        internal SearchResults(Query query, IEnumerable<SortField> sortField, LuceneExamineSearcher examineSearcher)
        {
            searcher = new IndexSearcher(new SimpleFSDirectory(examineSearcher.LuceneIndexFolder), true);
            if (sortField.Count() == 0)
            {
                collector = new AllHitsCollector(false, true);
                searcher.Search(query, collector); 
            }
            else
            {
                var topDocs = searcher.Search(query, null, searcher.MaxDoc() - 1, new Sort(sortField.ToArray()));
                collector = new AllHitsCollector(topDocs.scoreDocs);
            }
            this.TotalItemCount = collector.Count;
        }

        public int TotalItemCount { get; private set; }

        private Dictionary<int, SearchResult> docs = new Dictionary<int, SearchResult>();
        private SearchResult CreateSearchResult(Document doc, float score)
        {
            var sr = new SearchResult()
            {
                Id = int.Parse(doc.Get("id")),
                Score = score
            };

            //we can use lucene to find out the fields which have been stored for this particular document
            //I'm not sure if it'll return fields that have null values though
            var fields = doc.GetFields();
            //ignore our internal fields though
            foreach (Field field in fields
                .Cast<Field>()
                .Where(x => x.Name() != LuceneExamineIndexer.IndexNodeIdFieldName && x.Name() != LuceneExamineIndexer.IndexTypeFieldName))
            {
                sr.Fields.Add(field.Name(), doc.Get(field.Name()));
            }

            return sr;
        }

        /// <summary>
        /// Skips to a particular point in the search results.
        /// </summary>
        /// <remarks>
        /// This allows for lazy loading of the results paging. We don't go into Lucene until we have to.
        /// </remarks>
        /// <param name="skip">The skip.</param>
        /// <returns></returns>
        public IEnumerable<SearchResult> Skip(int skip)
        {
            for (int i = skip; i < this.TotalItemCount; i++)
            {
                if (!docs.ContainsKey(i))
                {
                    var docId = collector.GetDocId(i);
                    var doc = searcher.Doc(docId);
                    var score = collector.GetDocScore(i);

                    docs.Add(i, CreateSearchResult(doc, score));
                }
                //using yield return means if the user breaks out we wont keep going
                //only load what we need to load!
                yield return docs[i];
            }
        }

        #region IEnumerable<SearchResult> Members

        public IEnumerator<SearchResult> GetEnumerator()
        {
            //if we're going to Enumerate from this itself we're not going to be skipping
            //so unless we made it IQueryable we can't do anything better than start from 0
            return this.Skip(0).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
