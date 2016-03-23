using System.Diagnostics;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;

namespace Examine.LuceneEngine.Indexing
{
    //TODO: I cannot make this one work even though there is nothing special going on in it
    //public class DefaultHighlighter : IHighlighter
    //{
    //    private readonly Analyzer _analyzer;
    //    private readonly string _fieldName;
    //    private readonly Searcher _searcher;
    //    private readonly Highlighter _highlighter;

    //    public DefaultHighlighter(Query query, Analyzer analyzer, string fieldName, Searcher searcher)
    //    {
    //        var formatter = new SimpleHTMLFormatter("<span class='search-highlight'>", "</span>");

    //        _analyzer = analyzer;
    //        _fieldName = fieldName;
    //        _searcher = searcher;
    //        var fragmenter = new SimpleFragmenter(100);
    //        var scorer = new QueryScorer(query);
    //        _highlighter = new Highlighter(formatter, scorer)
    //        {
    //            TextFragmenter = fragmenter
    //        };
    //    }

    //    public string Highlight(int docId)
    //    {
    //        var document = _searcher.Doc(docId);
    //        var text = string.Join("\r\n",
    //                               document.GetFields(_fieldName)
    //                                       .Select(f => f.StringValue));
    //        try
    //        {
    //            return _highlighter.GetBestFragment(_analyzer, _fieldName, text);
    //        }
    //        catch (InvalidTokenOffsetsException ex)
    //        {
    //            Trace.TraceError("An error occurred in {0}.{1} _highlighter.GetBestFragment: {2}", nameof(DefaultHighlighter), nameof(Highlight), ex);

    //            return string.Empty;
    //        } 
    //    }
    //}
}
