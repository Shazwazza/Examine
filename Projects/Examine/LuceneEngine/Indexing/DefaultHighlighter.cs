using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Highlight;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing
{
    public class DefaultHighlighter : IHighlighter
    {
        private readonly Analyzer _analyzer;
        private readonly IIndexValueType _valueType;
        private Highlighter _highlighter;

        public DefaultHighlighter(Query query, Analyzer analyzer, IIndexValueType valueType)
        {
            var formatter = new SimpleHTMLFormatter("<em>", "</em>");


            _analyzer = analyzer;
            _valueType = valueType;
            var fragmenter = new SimpleFragmenter(100);
            var scorer = new QueryScorer(query);
            _highlighter = new Highlighter(formatter, scorer);
            _highlighter.SetTextFragmenter(fragmenter);
        }

        public string Highlight(Document document)
        {
            var text = string.Join("\r\n",
                                   document.GetFields(_valueType.FieldName)
                                           .Select(f => f.StringValue()));
            return _highlighter.GetBestFragment(_analyzer, _valueType.FieldName, text);
        }
    }
}
