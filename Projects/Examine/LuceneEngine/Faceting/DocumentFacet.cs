namespace Examine.LuceneEngine.Faceting
{
    /// <summary>
    /// Used for facet reading. IndexReaderData transforms DocumentFacets to FacetLevels.
    /// </summary>
    public class DocumentFacet
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="termBased"></param>
        /// <param name="key"></param>
        /// <param name="level"></param>
        public DocumentFacet(int documentId, bool termBased, FacetKey key, float level)
        {
            DocumentId = documentId;
            TermBased = termBased;
            Key = key;
            Level = level;
        }

        /// <summary>
        /// The ID of the document
        /// </summary>
        public int DocumentId { get; private set; }

        /// <summary>
        /// True if the facet is directly based on a term in the index
        /// </summary>
        public bool TermBased { get; private set; }

        /// <summary>
        /// The key for the facet
        /// </summary>
        public FacetKey Key { get; private set; }

        /// <summary>
        /// The level/size of the facet. A large value indicates that the facet applies "more" to the document.
        /// </summary>
        public float Level { get; private set; }
    }
}