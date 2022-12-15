using Examine.Lucene.Search;

namespace Examine.Search
{
    public interface IFacetLongField : IFacetField
    {
        Int64Range[] LongRanges { get; }
    }
}
