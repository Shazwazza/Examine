using Examine.Lucene.Search;

namespace Examine.Lucene
{
    /// <summary>
    /// Extensions for configuring Similarity on a Lucene Index
    /// </summary>
    public static class LuceneIndexOptionsExtensions
    {
        /// <summary>
        /// Add default set of Lucene Similarities. See <see cref="ExamineLuceneSimilarityNames"/> for Similarity Names
        /// </summary>
        /// <param name="similarityDefinitions"></param>
        /// <returns></returns>
        public static SimilarityDefinitionCollection AddExamineLuceneSimilarities(this SimilarityDefinitionCollection similarityDefinitions)
        {
            similarityDefinitions.AddOrUpdate(new SimilarityDefinition(ExamineLuceneSimilarityNames.ExamineDefault, ExamineLuceneSimilarityNames.ExamineDefault));
            similarityDefinitions.AddOrUpdate(new SimilarityDefinition(ExamineLuceneSimilarityNames.Classic, ExamineLuceneSimilarityNames.Classic));
            similarityDefinitions.AddOrUpdate(new SimilarityDefinition(ExamineLuceneSimilarityNames.BM25, ExamineLuceneSimilarityNames.BM25));
            similarityDefinitions.AddOrUpdate(new SimilarityDefinition(ExamineLuceneSimilarityNames.LMDirichlet, ExamineLuceneSimilarityNames.LMDirichlet));
            similarityDefinitions.AddOrUpdate(new SimilarityDefinition(ExamineLuceneSimilarityNames.LMJelinekMercerTitle, ExamineLuceneSimilarityNames.LMJelinekMercerTitle));
            similarityDefinitions.AddOrUpdate(new SimilarityDefinition(ExamineLuceneSimilarityNames.LMJelinekMercerLongText, ExamineLuceneSimilarityNames.LMJelinekMercerLongText));
            return similarityDefinitions;
        }
    }
}
