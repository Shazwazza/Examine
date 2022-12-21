using Examine.Lucene.Search;

namespace Examine.Search
{
    public interface IFacetFloatField : IFacetField
    {
        FloatRange[] FloatRanges { get; }
    }
}
