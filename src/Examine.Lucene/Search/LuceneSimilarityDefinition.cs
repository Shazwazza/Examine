using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Lucene.NET Similarity Defintion
    /// </summary>
    public class LuceneSimilarityDefinition : LuceneSimilarityDefinitionBase
    {
        private readonly Similarity _similarity;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="name">Similarity Name</param>
        /// <param name="similarity">Lucene Similarity</param>
        public LuceneSimilarityDefinition(string name, Similarity similarity) : base(name)
        {
            _similarity = similarity;
        }

        /// <inheritdoc/>
        public override Similarity GetSimilarity() => _similarity;
    }
}
