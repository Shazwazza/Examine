using Examine.Search;
using Lucene.Net.Search;

namespace Examine.Lucene.Indexing
{
    public interface ISpatialIndexFieldValueTypeBase
    {
        IExamineSpatialShapeFactory ExamineSpatialShapeFactory { get; }

        SortField ToSpatialDistanceSortField(SortableField sortableField, SortDirection sortDirection);
    }
}