using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing
{
    public interface IIndexValueType
    {
        string FieldName { get; }

        bool Store { get; }

        void SetupAnalyzers(PerFieldAnalyzerWrapper analyzer);

        void AddValue(Document doc, object value);

        //void AnalyzeReader(ReaderData readerData);

        Query GetQuery(string query, Searcher searcher);

        //IHighlighter GetHighlighter(Query query, Searcher searcher, FacetsLoader facetsLoader);

    }
}