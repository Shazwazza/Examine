using Examine.Lucene.Search;
using Lucene.Net.Facet.Range;

namespace Examine.Search
{
    public interface IFacetLongField : IFacetField
    {
        Int64Range[] LongRanges { get; }
    }
}
