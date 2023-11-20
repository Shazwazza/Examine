using System;
using Examine.Lucene.Indexing;

namespace Examine.Lucene
{
    /// <inheritdoc />
    /// <summary>
    /// A factory to create a <see cref="ISimilarityTypeFactory" /> for a similarity name based on a Func delegate
    /// </summary>
    public class DelegateSimilarityTypeFactory : ISimilarityTypeFactory
    {
        private readonly Func<IIndexSimilarityType> _factory;

        /// <inheritdoc/>
        public DelegateSimilarityTypeFactory(Func<IIndexSimilarityType> factory)
        {
            _factory = factory;
        }

        /// <inheritdoc/>
        public IIndexSimilarityType Create() => _factory();
    }
}
