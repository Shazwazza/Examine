using Examine.Lucene.Indexing;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Suggester Context for LuceneSuggester
    /// </summary>
    public interface ISuggesterContext
    {
        // <summary>
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
    }
}
