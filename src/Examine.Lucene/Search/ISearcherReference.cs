using System;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Represents a searcher reference
    /// </summary>
    /// <remarks>Dispose will release it from the manager</remarks>
    public interface ISearcherReference : IDisposable
    {
        /// <summary>
        /// The index searcher
        /// </summary>
        IndexSearcher IndexSearcher { get; }
    }
}
