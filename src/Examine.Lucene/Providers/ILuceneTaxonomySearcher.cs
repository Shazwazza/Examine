using System;
using Examine.Lucene.Search;

namespace Examine.Lucene.Providers
{
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
    }
}
