using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    public class LuceneSimilarityDefinition : SimilarityDefinition
    {
        private readonly Similarity _similarity;

        public LuceneSimilarityDefinition(string name, Similarity similarity) : base(name)
        {
            _similarity = similarity;
        }

        public Similarity GetSimilarity() => _similarity;
    }
}
