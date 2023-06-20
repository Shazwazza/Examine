using System.Collections.Generic;

namespace Examine
{
    /// <summary>
    /// Represents a search result
    /// </summary>
    public interface ISearchResult
    {
        /// <summary>
        /// The id of the search result
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The score of the search result
        /// </summary>
        float Score { get; }

        /// <summary>
        /// Returns the values in the result
        /// </summary>
        IReadOnlyDictionary<string, string> Values { get; }

        /// <summary>
        /// Returns the values in the result
        /// </summary>
        /// <remarks>
        /// This is used to retrieve multiple values per field if there are any
        /// </remarks>
        IReadOnlyDictionary<string, IReadOnlyList<string>> AllValues { get; }

        /// <summary>
        /// If a single field was indexed with multiple values this will return those values, otherwise it will just return the single 
        /// value stored for that field. If the field is not found it returns an empty collection.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IEnumerable<string> GetValues(string key);

        /// <summary>
        /// Returns the key value pair for the index specified
        /// </summary>
        /// <param name="resultIndex"></param>
        /// <returns></returns>
        KeyValuePair<string, string> this[int resultIndex] { get; }

        /// <summary>
        /// Returns the value for the key specified
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string this[string key] { get; }
    }
}
