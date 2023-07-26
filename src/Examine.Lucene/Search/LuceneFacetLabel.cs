using Lucene.Net.Facet.Taxonomy;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Lucene Facet Label
    /// </summary>
    public class LuceneFacetLabel : Examine.Search.IFacetLabel
    {
        private readonly FacetLabel _facetLabel;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="facetLabel">Lucene Facet Label</param>
        public LuceneFacetLabel(FacetLabel facetLabel)
        {
            _facetLabel = facetLabel;
        }

        /// <inheritdoc/>
        public string[] Components => _facetLabel.Components;

        /// <inheritdoc/>
        public int Length => _facetLabel.Length;

        /// <inheritdoc/>
        public int CompareTo(Examine.Search.IFacetLabel other) => _facetLabel.CompareTo(new FacetLabel(other.Components));

        /// <inheritdoc/>
        public Examine.Search.IFacetLabel Subpath(int length) => new LuceneFacetLabel(_facetLabel.Subpath(length));
    }
}
