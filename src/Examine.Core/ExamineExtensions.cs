namespace Examine
{
    /// <summary>
    /// Static methods to help query umbraco xml
    /// </summary>
    public static class ExamineExtensions
    {
        public static void DeleteFromIndex(this IIndex index, string itemId)
        {
            index.DeleteFromIndex(new[] {itemId});
        }

        /// <summary>
        /// Method to re-index specific data
        /// </summary>
        /// <param name="index"></param>
        /// <param name="node"></param>
        public static void IndexItem(this IIndex index, ValueSet node)
        {
            index.IndexItems(new[] { node });
        }

    }
}
