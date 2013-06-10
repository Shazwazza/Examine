using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing.Analyzers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing.ValueTypes
{
    public class FullTextType : IndexValueTypeBase
    {
        public FullTextType(string fieldName)
            : base(fieldName, true)
        {
        }

        Analyzer _analyzer = new LowercaseAccentRemovingWhitespaceAnalyzer();
        public override void SetupAnalyzers(Lucene.Net.Analysis.PerFieldAnalyzerWrapper analyzer)
        {
            base.SetupAnalyzers(analyzer);

            analyzer.AddAnalyzer(FieldName, analyzer);
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            doc.Add(new Field(FieldName, "" + value, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
        }

        public override Query GetQuery(string query, Searcher searcher, FacetsLoader facetsLoader)
        {
            if (query == null)
            {
                return null;
            }

            var tokenStream = _analyzer.TokenStream("SearchText", new StringReader(query));
            var termAttribute = (TermAttribute)tokenStream.GetAttribute(typeof(TermAttribute));
            

            var bq = new BooleanQuery();
            while (tokenStream.IncrementToken())
            {
                var term = termAttribute.Term();
                var directMatch = new TermQuery(new Term(FieldName, term));
                if (term.Length >= 3)
                {
                    var bqInner = new BooleanQuery();
                    bqInner.Add(directMatch, BooleanClause.Occur.SHOULD);
                    //TODO: This is where all kinds of awesome should happen, including spell checking etc.
                    var pq = new PrefixQuery(new Term(FieldName, term));
                    pq.SetRewriteMethod(new MultiTermQuery.ConstantScoreAutoRewrite());
                    bqInner.Add(pq, BooleanClause.Occur.SHOULD);

                    bq.Add(bqInner, BooleanClause.Occur.MUST);
                }
                else
                {
                    bq.Add(directMatch, BooleanClause.Occur.MUST);
                }
            }

            return bq.Clauses().Count > 0 ? bq : null;
        }

        public override IHighlighter GetHighlighter(Query query, Searcher searcher, FacetsLoader facetsLoader)
        {
            return new DefaultHighlighter(query, _analyzer, this);
        }
    }
}
