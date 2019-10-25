using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing
{
    /// <summary>
    /// Defines how a field value is stored in the index and is responsible for generating a query for the field when a managed query is used
    /// </summary>
    public interface IIndexFieldValueType
    {
        string FieldName { get; }

        /// <summary>
        /// Returns the sortable field name or null if the value isn't sortable
        /// </summary>
        string SortableFieldName { get; }

        bool Store { get; }

        void SetupAnalyzers(PerFieldAnalyzerWrapper analyzer);

        void AddValue(Document doc, object value);
        
        Query GetQuery(string query, Searcher searcher);

        //IHighlighter GetHighlighter(Query query, Searcher searcher, FacetsLoader facetsLoader);

    }
}