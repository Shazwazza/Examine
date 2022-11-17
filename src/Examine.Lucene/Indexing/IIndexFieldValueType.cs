using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Examine.Lucene.Indexing
{
    /// <summary>
    /// Defines how a field value is stored in the index and is responsible for generating a query for the field when a managed query is used
    /// </summary>
    public interface IIndexFieldValueType
    {
        /// <summary>
        /// The field name
        /// </summary>
        string FieldName { get; }

        /// <summary>
        /// Returns the sortable field name or null if the value isn't sortable
        /// </summary>
        /// <remarks>By default it will not be sortable</remarks>
        string SortableFieldName { get; }

        /// <summary>
        /// Should the value be stored
        /// </summary>
        bool Store { get; }

        /// <summary>
        /// Returns the analyzer for this field type, or null to use the default
        /// </summary>
        Analyzer Analyzer { get; }

        /// <summary>
        /// Adds a value to the document
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="value"></param>
        void AddValue(Document doc, object value);

        /// <summary>
        /// Gets a query as <see cref="Query"/>
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Query GetQuery(string query);

        //IHighlighter GetHighlighter(Query query, Searcher searcher, FacetsLoader facetsLoader);

    }
}
