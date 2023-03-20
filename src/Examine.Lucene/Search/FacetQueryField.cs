using Examine.Search;

namespace Examine.Lucene.Search
{
    public class FacetQueryField : IFacetQueryField
    {
        private readonly FacetFullTextField _field;

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

        public IFacetQueryField SetPath(string[] path)
        {
            _field.Path = path;

            return this;
        }
    }
}
