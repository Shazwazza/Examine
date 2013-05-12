namespace Examine.LuceneEngine.Faceting
{
    /// <summary>
    /// Used for facet reading. IndexReaderData transforms DocumentFacets to FacetLevels.
    /// </summary>
    public class DocumentFacet
    {
        /// <summary>
        /// The ID of the document
        /// </summary>
        public int DocumentId { get; set; }

        /// <summary>
        /// True if the facet is directly based on a term in the index
        /// </summary>
        public bool TermBased { get; set; }

        /// <summary>
        /// The key for the facet
        /// </summary>
        public FacetKey Key { get; set; }

        /// <summary>
        /// The level/size of the facet. A large value indicates that the facet applies "more" to the document.
        /// </summary>
        public float Level { get; set; }
    }
}