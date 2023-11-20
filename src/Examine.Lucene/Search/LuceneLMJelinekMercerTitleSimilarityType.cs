using System;
using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// LMJelinekMercerSimilarity with parameter 0.1f which is suitable for title searches. <see cref="LMJelinekMercerSimilarity"/>
    /// </summary>
    public class LuceneLMJelinekMercerTitleSimilarityType : LuceneSimilarityTypeBase
    {
        private readonly Lazy<LMJelinekMercerSimilarity> _similarityLazy = new Lazy<LMJelinekMercerSimilarity>(() => new LMJelinekMercerSimilarity(0.1f));

        /// <summary>
        /// Constructor
        /// </summary>
        public LuceneLMJelinekMercerTitleSimilarityType() : base(ExamineLuceneSimilarityNames.LMDirichlet)
        {
        }

        /// <inheritdoc/>
        public override Similarity GetSimilarity() => _similarityLazy.Value;
    }
}
