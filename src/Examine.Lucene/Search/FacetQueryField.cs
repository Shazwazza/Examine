using Examine.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Represents a default facet query field (FullText)
    /// </summary>
    public class FacetQueryField : IFacetQueryField
    {
        private readonly FacetFullTextField _field;

        /// <inheritdoc/>
        public FacetQueryField(FacetFullTextField field)
        {
            _field = field;
        }

        /// <inheritdoc/>
        public IFacetQueryField MaxCount(int count)
        {
            _field.MaxCount = count;

            return this;
        }

        /// <inheritdoc/>
        public IFacetQueryField SetPath(params string[] path)
        {
            _field.Path = path;

            return this;
        }
    }
}
