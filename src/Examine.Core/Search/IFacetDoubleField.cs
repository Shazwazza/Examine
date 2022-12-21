using Examine.Lucene.Search;

namespace Examine.Search
{
    public interface IFacetDoubleField : IFacetField
    {
        DoubleRange[] DoubleRanges { get; }
    }
}
