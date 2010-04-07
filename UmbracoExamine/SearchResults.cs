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

        internal SearchResults(Query query, LuceneExamineSearcher examineSearcher)
        {
            collector = new AllHitsCollector(false, true);
            searcher = new IndexSearcher(new SimpleFSDirectory(examineSearcher.LuceneIndexFolder), true);
            searcher.Search(query, collector);

            this.TotalItemCount = collector.Count;
        }

        public int TotalItemCount { get; private set; }

        private Dictionary<int, SearchResult> docs = new Dictionary<int, SearchResult>();
        private SearchResult CreateSearchResult(Document doc, float score)
        {
            var sr = new SearchResult()
            {
                Id = int.Parse(doc.GetField("id").StringValue()),
                Score = score
            };

            var fields = doc.GetFields();
            foreach (Field field in fields
                .Cast<Field>()
                .Where(x => x.Name() != LuceneExamineIndexer.IndexNodeIdFieldName && x.Name() != LuceneExamineIndexer.IndexTypeFieldName))
            {
                sr.Fields.Add(field.Name(), field.StringValue());
            }

            return sr;
        }

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
                yield return docs[i];
            }
        }

        #region IEnumerable<SearchResult> Members

        public IEnumerator<SearchResult> GetEnumerator()
        {
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
