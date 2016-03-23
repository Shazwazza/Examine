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
    //TODO: What does this do? I'm disabling this for now
    //public sealed class AutoSuggestType : IndexValueTypeBase
    //{
    //    private Analyzer _queryAnalyzer;
    //    private Analyzer _indexAnalyzer;

    //    public AutoSuggestType(string fieldName)
    //        : base(fieldName, true)
    //    {
    //        _queryAnalyzer = new LowercaseAccentRemovingWhitespaceAnalyzer();
    //        _indexAnalyzer = new PrefixAnalyzer(_queryAnalyzer);            
    //    }

    //    public override void SetupAnalyzers(PerFieldAnalyzerWrapper analyzer)
    //    {
    //        base.SetupAnalyzers(analyzer);

    //        analyzer.AddAnalyzer(FieldName, new PrefixAnalyzer(_queryAnalyzer));
    //    }

    //    protected override void AddSingleValue(Document doc, object value)
    //    {
    //        doc.Add(new Field(FieldName, "" + value, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
    //    }

    //    public override Query GetQuery(string query, Searcher searcher, FacetsLoader facetsLoader, IManagedQueryParameters parameters)
    //    {
    //        if (query == null)
    //        {
    //            return null;
    //        }

    //        var tokenStream = _queryAnalyzer.TokenStream("SearchText", new StringReader(query));
    //        var termAttribute = tokenStream.AddAttribute<ITermAttribute>();


    //        var bq = new BooleanQuery();
    //        while (tokenStream.IncrementToken())
    //        {
    //            var term = termAttribute.Term;
    //            var bqInner = new BooleanQuery();
    //            var directMatch = new TermQuery(new Term(FieldName, term));
    //            directMatch.Boost = 10;
    //            bqInner.Add(directMatch, Occur.SHOULD);                
    //            var pq = new TermQuery(new Term(FieldName, term + "*"));
    //            bqInner.Add(pq, Occur.SHOULD);

    //            bq.Add(bqInner, Occur.MUST);
    //        }

    //        return bq.Clauses.Count > 0 ? bq : null;
    //    }

    //    public override IHighlighter GetHighlighter(Query query, Searcher searcher, FacetsLoader facetsLoader)
    //    {
    //        return new DefaultHighlighter(query, _indexAnalyzer, FieldName, searcher);
    //    }
    //}
}
