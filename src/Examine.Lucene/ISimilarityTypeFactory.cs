using Examine.Lucene.Indexing;

namespace Examine.Lucene
{
    /// <summary>
    /// A factory to create a <see cref="IIndexSimilarityType"/> for a similarity name
    /// </summary>
    public interface ISimilarityTypeFactory
    {
        /// <summary>
        /// Creates a <see cref="IIndexSimilarityType"/> for a similarity name
        /// </summary>
        /// <returns></returns>
        IIndexSimilarityType Create();
    }
}
