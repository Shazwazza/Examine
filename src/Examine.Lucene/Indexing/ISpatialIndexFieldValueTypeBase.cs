using System;
using Examine.Search;
using Lucene.Net.Search;

namespace Examine.Lucene.Indexing
{
    public interface ISpatialIndexFieldValueTypeBase
    {
        IExamineSpatialShapeFactory ExamineSpatialShapeFactory { get; }

        SortField ToSpatialDistanceSortField(SortableField sortableField, SortDirection sortDirection);

        Query GetQuery(string field, ExamineSpatialOperation spatialOperation, Func<IExamineSpatialShapeFactory, IExamineSpatialShape> shape);
    }
}
