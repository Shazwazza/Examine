using System;
using Examine.Lucene.Suggest;
using Examine.Suggest;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Examine.Lucene.Indexing
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

        /// <summary>
        /// Returns the index time analyzer for this field type, or null to use the default
        /// </summary>
        Analyzer Analyzer { get; }

        /// <summary>
        /// Returns the search time analyzer for this field type, or null to use the default
        /// </summary>
        Analyzer SearchAnalyzer { get; }

        /// <summary>
        /// Returns the lookup for this field type, or null to use the default
        /// </summary>
        Func<IIndexReaderReference, SuggestionOptions, string, LuceneSuggestionResults> Lookup { get; }

        void AddValue(Document doc, object value);
        
        Query GetQuery(string query);

        //IHighlighter GetHighlighter(Query query, Searcher searcher, FacetsLoader facetsLoader);

    }
}
