using System;
using Examine.Search;

namespace Examine.Lucene.Providers
{
    /// <summary>
    /// Lucene Taxonomy Searcher
    /// </summary>
    public interface ILuceneTaxonomySearcher : ISearcher, IDisposable
    {
        /// <summary>
        /// The number of categories in the Taxonomy
        /// </summary>
        int CategoryCount { get; }

        /// <summary>
        /// Returns the Ordinal for the dim and path
        /// </summary>
        int GetOrdinal(string dim, string[] path);

        /// <summary>
        /// Given a dimensions ordinal (id), get the Path.
        /// </summary>
        /// <param name="ordinal">Demension ordinal (id)</param>
        /// <returns>Path</returns>
        IFacetLabel GetPath(int ordinal);
    }
}
