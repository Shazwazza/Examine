using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Indexing
{
    /// <summary>
    /// Responsible for generating a Similarity
    /// </summary>
    public interface IIndexSimilarityType
    {
        /// <summary>
        /// The Similarity name
        /// </summary>
        string SimilarityName { get; }

        /// <summary>
        /// Gets the Lucene.NET Similarity Definition
        /// </summary>
        /// <returns></returns>
        public Similarity GetSimilarity();
    }
}
