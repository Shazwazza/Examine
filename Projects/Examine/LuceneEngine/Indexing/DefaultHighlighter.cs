using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;

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
            _highlighter = new Highlighter(formatter, scorer)
            {
                TextFragmenter = fragmenter
            };
        }

        public string Highlight(int docId)
        {
            var document = _searcher.Doc(docId);
            var text = string.Join("\r\n",
                                   document.GetFields(_fieldName)
                                           .Select(f => f.StringValue));
            try
            {
                return _highlighter.GetBestFragment(_analyzer, _fieldName, text);
            }
            catch (InvalidTokenOffsetsException)
            {
                //TODO: This seems like a bug, happens when there are strange chars
                return string.Empty;
            } 
        }
    }
}
