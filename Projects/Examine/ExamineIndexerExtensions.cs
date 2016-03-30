using Examine.LuceneEngine.Indexing;

namespace Examine
{
    public static class ExamineIndexerExtensions
    {
        /// <summary>
        /// Method to re-index specific data
        /// </summary>
        /// <param name="indexer"></param>
        /// <param name="node"></param>
        public static void IndexItem(this IExamineIndexer indexer, ValueSet node)
        {
            indexer.IndexItems(new[] {node});
        }
    }
}