using System;
using Examine.Search;
using Lucene.Net.Search;

namespace Examine.Lucene.Indexing
{
    /// <summary>
    /// Spatial Index Field Value Type
    /// </summary>
    public interface ISpatialIndexFieldValueTypeBase : ISpatialIndexFieldValueTypeShapesBase
    {
        /// <summary>
        /// Converts an Examine Spatial SortableField to a Lucene SortField
        /// </summary>
        /// <param name="sortableField"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        SortField ToSpatialDistanceSortField(SortableField sortableField, SortDirection sortDirection);

        /// <summary>
        /// Gets a spatial query as a Lucene Query
        /// </summary>
        /// <param name="field"></param>
        /// <param name="spatialOperation"></param>
        /// <param name="shape"></param>
        /// <returns></returns>
        Query GetQuery(string field, ExamineSpatialOperation spatialOperation, Func<ISpatialShapeFactory, ISpatialShape> shape);

        /// <summary>
        /// Gets a spatial filer as a Lucene Filter
        /// </summary>
        /// <param name="field"></param>
        /// <param name="spatialOperation"></param>
        /// <param name="shape"></param>
        /// <returns></returns>
        Filter GetFilter(string field, ExamineSpatialOperation spatialOperation, Func<ISpatialShapeFactory, ISpatialShape> shape);
    }
}
