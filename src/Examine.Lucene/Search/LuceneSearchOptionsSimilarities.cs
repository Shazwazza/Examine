using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Common Similarities for Lucene.NET Search
    /// </summary>
    public static class LuceneSearchOptionsSimilarities
    {
        /// <summary>
        /// Default Similarity for Examine Lucene.
        /// </summary>
        /// <remarks>In Examine V3 and V4, this is <see cref="DefaultSimilarity"/>. In Examine V5 this will change to <see cref="BM25Similarity"/></remarks>
        public static readonly Similarity ExamineDefault = new DefaultSimilarity();

        /// <summary>
        /// Classic Similarity for Lucene. <see cref="DefaultSimilarity"/>
        /// </summary>
        public static readonly Similarity Classic = new DefaultSimilarity();

        /// <summary>
        /// BM25Similarity with default parameters for Lucene. <see cref="BM25Similarity"/>
        /// </summary>
        public static readonly Similarity BM25 = new BM25Similarity();

        /// <summary>
        /// LMDirichletSimilarity with default parameters for Lucene. <see cref="LMDirichletSimilarity"/>
        /// </summary>
        public static readonly Similarity LMDirichlet = new LMDirichletSimilarity();

        /// <summary>
        /// LMJelinekMercerSimilarity with parameter 0.1f which is suitable for title searches. <see cref="LMJelinekMercerSimilarity"/>
        /// </summary>
        public static readonly Similarity LMJelinekMercerTitle = new LMJelinekMercerSimilarity(0.1f);

        /// <summary>
        /// LMJelinekMercerSimilarity with parameter 0.7f which is suitable for long text searches. <see cref="LMJelinekMercerSimilarity"/>
        /// </summary>
        public static readonly Similarity LMJelinekMercerLongText = new LMJelinekMercerSimilarity(0.7f);
    }
}
