namespace Examine.Search
{
    public class FacetQueryField : IFacetQueryField
    {
        private readonly FacetFullTextField _field;

        public FacetQueryField(FacetFullTextField field)
        {
            _field = field;
        }

        public IFacetQueryField MaxCount(int count)
        {
            _field.MaxCount = count;

            return this;
        }

        /// <inheritdoc/>
        public IFacetQueryField Path(params string[] path)
        {
            _field.Path = path;

            return this;
        }
    }
}
