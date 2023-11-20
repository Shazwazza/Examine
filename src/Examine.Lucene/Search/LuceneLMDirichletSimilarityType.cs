using System;
using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// LMDirichletSimilarity with default parameters for Lucene. <see cref="LMDirichletSimilarity"/>
    /// </summary>
    public class LuceneLMDirichletSimilarityType : LuceneSimilarityTypeBase
    {
        private readonly Lazy<LMDirichletSimilarity> _similarityLazy = new Lazy<LMDirichletSimilarity>(() => new LMDirichletSimilarity());

        /// <summary>
        /// Constructor
        /// </summary>
        public LuceneLMDirichletSimilarityType() : base(ExamineLuceneSimilarityNames.LMDirichlet)
        {
        }

        /// <inheritdoc/>
        public override Similarity GetSimilarity() => _similarityLazy.Value;
    }
}
