using System.Collections.Generic;
using System.IO;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing
{
    public class FullTextType : IndexValueTypeBase
    {
        private readonly bool _sortable;
        public int TermExpansions { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="sortable"></param>
        public FullTextType(string fieldName, bool sortable = false)
            : base(fieldName, true)
        {
            _sortable = sortable;
            TermExpansions = 25;
        }

        private readonly Analyzer _analyzer = new LowercaseAccentRemovingWhitespaceAnalyzer();
        public override void SetupAnalyzers(PerFieldAnalyzerWrapper analyzer)
        {
            base.SetupAnalyzers(analyzer);

            analyzer.AddAnalyzer(FieldName, _analyzer);
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            doc.Add(new Field(FieldName, "" + value, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));

            if (_sortable)
            {
                doc.Add(new Field(LuceneIndexer.SortedFieldNamePrefix + FieldName, "" + value,
                    Field.Store.YES,
                    Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            }
        }


        public override Query GetQuery(string query, Searcher searcher, IManagedQueryParameters parameters)
        {
            //TODO: Fix this!

            if (query == null)
            {
                return null;
            }

            var tokenStream = _analyzer.TokenStream("SearchText", new StringReader(query));
            var termAttribute = tokenStream.AddAttribute<ITermAttribute>();


            var bq = new BooleanQuery();
            while (tokenStream.IncrementToken())
            {
                var term = termAttribute.Term;
                var directMatch = new TermQuery(new Term(FieldName, term));
                if (term.Length >= 3)
                {
                    directMatch.Boost = 10;

                    var bqInner = new BooleanQuery();
                    bqInner.Add(directMatch, Occur.SHOULD);


                    //var pq = new PrefixQuery(new Term(FieldName, term));
                    //pq.SetRewriteMethod(MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE);
                    //foreach (var r in searcher.GetSubSearchers().Select(s => s.GetIndexReader()))
                    //{
                    //    bqInner.Add(pq.Rewrite(r), Occur.SHOULD);
                    //}

                    //var pops = GetPopularTerms(term, searcher, facetsLoader).GetTopItems(TermExpansions,
                    //    new LambdaComparer<KeyValuePair<string, double>>((x, y) =>x.Value.CompareTo(y.Value))).ToArray();

                    //if (pops.Length > 0)
                    //{
                    //    var max = pops.Max(p => p.Value);
                    //    foreach (var p in pops)
                    //    {
                    //        var pq = new TermQuery(new Term(FieldName, p.Key));
                    //        pq.Boost = ((float) (p.Value/max));
                    //        bqInner.Add(pq, Occur.SHOULD);
                    //    }
                    //}

                    //TODO: This is where all kinds of awesome should happen, including spell checking etc.
                    //var pq = new PrefixQuery(new Term(FieldName, term));

                    //pq.SetRewriteMethod(MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE);

                    //foreach (var r in searcher.GetSubSearchers().Select(s => s.GetIndexReader()))
                    //{
                    //    bqInner.Add(pq.Rewrite(r), Occur.SHOULD);
                    //}

                    //pq.SetRewriteMethod(MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE);
                    //foreach (var ixs in searcher.GetSubSearchers())
                    //{
                    //    bqInner.Add(pq.Rewrite(ixs.GetIndexReader()), Occur.SHOULD);                        
                    //}                    


                    bq.Add(bqInner, Occur.MUST);
                }
                else
                {
                    bq.Add(directMatch, Occur.MUST);
                }
            }

            return bq.Clauses.Count > 0 ? bq : null;
        }

    }
}
