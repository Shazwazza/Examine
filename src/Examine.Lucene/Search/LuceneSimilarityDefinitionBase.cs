using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Base Class for Lucene.NET Similarity Defintions
    /// </summary>
    public abstract class LuceneSimilarityDefinitionBase : SimilarityDefinition
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Similarity Name</param>
        public LuceneSimilarityDefinitionBase(string name) : base(name)
        {
        }

        /// <summary>
        /// Gets the Lucene.NET Similarity Definition
        /// </summary>
        /// <returns></returns>
        public abstract Similarity GetSimilarity();
    }
}
