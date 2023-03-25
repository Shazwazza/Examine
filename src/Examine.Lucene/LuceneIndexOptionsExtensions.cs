using Examine.Lucene.Search;

namespace Examine.Lucene
{
    public static class LuceneIndexOptionsExtensions
    {
        /// <summary>
        /// Add default set of Lucene Similarities. See <see cref="ExamineLuceneSimilarityNames"/> for Similarity Names
        /// </summary>
        /// <param name="similarityDefinitions"></param>
        /// <returns></returns>
        public static SimilarityDefinitionCollection AddExamineLuceneSimilarities(this SimilarityDefinitionCollection similarityDefinitions)
        {
            similarityDefinitions.AddOrUpdate(new ExamineLuceneDefaultSimilarityDefinition());
            similarityDefinitions.AddOrUpdate(new LuceneClassicSimilarityDefinition());
            similarityDefinitions.AddOrUpdate(new LuceneBM25imilarityDefinition());
            similarityDefinitions.AddOrUpdate(new LuceneLMDirichletSimilarityDefinition());
            similarityDefinitions.AddOrUpdate(new LuceneLMJelinekMercerTitleSimilarityDefinition());
            similarityDefinitions.AddOrUpdate(new LuceneLMJelinekMercerLongTextSimilarityDefinition());
            return similarityDefinitions;
        }
    }
}
