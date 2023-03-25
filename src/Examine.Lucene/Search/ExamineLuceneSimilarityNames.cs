using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Built in Similarity names for Examine Lucene
    /// </summary>
    public class ExamineLuceneSimilarityNames
    {
        /// <summary>
        /// Default Similarity for Examine Lucene.
        /// </summary>
        /// <remarks>In Examine V3, this is <see cref="DefaultSimilarity"/>. In Examine V4 this will change to <see cref="BM25Similarity"/></remarks>
        public const string ExamineDefault = "Examine.Default";

        /// <summary>
        /// Classic Similarity for Lucene. <see cref="DefaultSimilarity"/>
        /// </summary>
        public const string Classic = "Lucene.Classic";

        /// <summary>
        /// BM25Similarity with default parameters for Lucene. <see cref="BM25Similarity"/>
        /// </summary>
        public const string BM25 = "Lucene.BM25";

        /// <summary>
        /// LMDirichletSimilarity with default parameters for Lucene. <see cref="LMDirichletSimilarity"/>
        /// </summary>
        public const string LMDirichlet = "Lucene.LMDirichlet";

        /// <summary>
        /// LMJelinekMercerSimilarity with parameter 0.1f which is suitable for title searches. <see cref="LMJelinekMercerSimilarity"/>
        /// </summary>
        public const string LMJelinekMercerTitle = "Lucene.LMJelinekMercerTitle";

        /// <summary>
        /// LMJelinekMercerSimilarity with parameter 0.7f which is suitable for long text searches. <see cref="LMJelinekMercerSimilarity"/>
        /// </summary>
        public const string LMJelinekMercerLongText = "Lucene.LMJelinekMercerLongText";
    }
}
