using System;
using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// LMJelinekMercerSimilarity with parameter 0.7f which is suitable for long text searches. <see cref="LMJelinekMercerSimilarity"/>
    /// </summary>
    public class LuceneLMJelinekMercerLongTextSimilarityType : LuceneSimilarityTypeBase
    {
        private readonly Lazy<LMJelinekMercerSimilarity> _similarityLazy = new Lazy<LMJelinekMercerSimilarity>(() => new LMJelinekMercerSimilarity(0.7f));

        /// <summary>
        /// Constructor
        /// </summary>
        public LuceneLMJelinekMercerLongTextSimilarityType() : base(ExamineLuceneSimilarityNames.LMDirichlet)
        {
        }

        /// <inheritdoc/>
        public override Similarity GetSimilarity() => _similarityLazy.Value;
    }
}
