using System.Collections.Generic;
using Examine.Lucene.Indexing;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Represents a search context
    /// </summary>
    public interface ISearchContext
    {
        /// <summary>
        /// Gets the searcher of the context
        /// </summary>
        /// <returns></returns>
        ISearcherReference GetSearcher();

        /// <summary>
        /// The searchable fields of a search context
        /// </summary>
        string[] SearchableFields { get; }

        /// <summary>
        /// Gets the field value type of a field name
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        IIndexFieldValueType? GetFieldValueType(string fieldName);

        /// <summary>
        /// Get Index Default Similarity
        /// </summary>
        /// <returns></returns>
        IIndexSimilarity? GetDefaultSimilarity();

        /// <summary>
        /// Get Index Similarity
        /// </summary>
        /// <param name="similarityName"></param>
        /// <returns></returns>
        IIndexSimilarity? GetSimilarity(string similarityName);
    }
}
