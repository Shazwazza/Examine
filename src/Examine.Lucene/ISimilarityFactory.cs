using Examine.Lucene.Indexing;

namespace Examine.Lucene
{
    /// <summary>
    /// A factory to create a <see cref="IIndexSimilarity"/> for a similarity name
    /// </summary>
    public interface ISimilarityFactory
    {
        /// <summary>
        /// Creates a <see cref="IIndexSimilarity"/> for a similarity name
        /// </summary>
        /// <returns></returns>
        IIndexSimilarity Create();
    }
}
