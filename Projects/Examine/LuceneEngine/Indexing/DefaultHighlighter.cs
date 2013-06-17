using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Highlight;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing
{
    public class DefaultHighlighter : IHighlighter
    {
        private readonly Analyzer _analyzer;
        private readonly string _fieldName;
        private readonly Searcher _searcher;
        private Highlighter _highlighter;

        public DefaultHighlighter(Query query, Analyzer analyzer, string fieldName, Searcher searcher)
        {
            var formatter = new SimpleHTMLFormatter("<em>", "</em>");

            _analyzer = analyzer;
            _fieldName = fieldName;
            _searcher = searcher;
            var fragmenter = new SimpleFragmenter(100);
            var scorer = new QueryScorer(query);
            _highlighter = new Highlighter(formatter, scorer);
            _highlighter.SetTextFragmenter(fragmenter);
        }

        public string Highlight(int docId)
        {
            var document = _searcher.Doc(docId);
            var text = string.Join("\r\n",
                                   document.GetFields(_fieldName)
                                           .Select(f => f.StringValue()));
            return _highlighter.GetBestFragment(_analyzer, _fieldName, text);
        }
    }
}
