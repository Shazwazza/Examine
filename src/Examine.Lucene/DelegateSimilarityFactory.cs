using System;
using Examine.Lucene.Indexing;

namespace Examine.Lucene
{
    /// <inheritdoc />
    /// <summary>
    /// A factory to create a <see cref="ISimilarityFactory" /> for a similarity name based on a Func delegate
    /// </summary>
    public class DelegateSimilarityFactory : ISimilarityFactory
    {
        private readonly Func<IIndexSimilarity> _factory;

        /// <inheritdoc/>
        public DelegateSimilarityFactory(Func<IIndexSimilarity> factory)
        {
            _factory = factory;
        }

        /// <inheritdoc/>
        public IIndexSimilarity Create() => _factory();
    }
}
