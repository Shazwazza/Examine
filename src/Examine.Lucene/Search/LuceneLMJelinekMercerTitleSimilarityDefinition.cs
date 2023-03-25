using System;
using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// LMJelinekMercerSimilarity with parameter 0.1f which is suitable for title searches. <see cref="LMJelinekMercerSimilarity"/>
    /// </summary>
    public class LuceneLMJelinekMercerTitleSimilarityDefinition : LuceneSimilarityDefinitionBase
    {
        private readonly Lazy<LMJelinekMercerSimilarity> _similarityLazy = new Lazy<LMJelinekMercerSimilarity>(() => new LMJelinekMercerSimilarity(0.1f));
        public LuceneLMJelinekMercerTitleSimilarityDefinition() : base(ExamineLuceneSimilarityNames.LMDirichlet)
        {
        }

        public override Similarity GetSimilarity() => _similarityLazy.Value;
    }
}
