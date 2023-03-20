using Examine.Lucene.Indexing;
using Examine.Suggest;
using Lucene.Net.Search.Suggest;
using Lucene.Net.Util;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Suggester Context for LuceneSuggester
    /// </summary>
    public interface ISuggesterContext
    {
        /// <summary>
        /// Retrieves a IndexReaderReference for the index the Suggester is for
        /// </summary>
        /// <returns></returns>
        IIndexReaderReference GetIndexReader();

        /// <summary>
        /// Gets the IIndexFieldValueType for a given Field Name in the Index.
        /// </summary>
        /// <param name="fieldName">Index Field Name</param>
        /// <returns></returns>
        IIndexFieldValueType GetFieldValueType(string fieldName);

        /// <summary>
        /// Gets the Suggester Definitions
        /// </summary>
        /// <returns></returns>
        SuggesterDefinitionCollection GetSuggesterDefinitions();

        /// <summary>
        /// Gets the Version of the Lucene Index
        /// </summary>
        /// <returns></returns>
        LuceneVersion GetLuceneVersion();

        /// <summary>
        /// Get the Suggester
        /// </summary>
        /// <param name="name">Suggester Name</param>
        /// <returns>Suggester</returns>
        TLookup GetSuggester<TLookup>(string name) where TLookup : Lookup;
    }

    public interface ILookupExecutor
    {
        ISuggestionResults ExecuteSuggester(string searchText, ISuggestionExecutionContext suggestionExecutionContext);
    }
}
