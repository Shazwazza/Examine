using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    public abstract class LuceneSimilarityDefinitionBase : SimilarityDefinition
    {
        public LuceneSimilarityDefinitionBase(string name) : base(name)
        {
        }

        public abstract Similarity GetSimilarity();
    }
}
