using System;
using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// LMDirichletSimilarity with default parameters for Lucene. <see cref="LMDirichletSimilarity"/>
    /// </summary>
    public class LuceneLMDirichletSimilarityDefinition : LuceneSimilarityDefinitionBase
    {
        private readonly Lazy<LMDirichletSimilarity> _similarityLazy = new Lazy<LMDirichletSimilarity>(() => new LMDirichletSimilarity());
        public LuceneLMDirichletSimilarityDefinition() : base(ExamineLuceneSimilarityNames.LMDirichlet)
        {
        }

        public override Similarity GetSimilarity() => _similarityLazy.Value;
    }
}
