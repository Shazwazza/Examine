namespace Examine.Search
{
    /// <summary>
    /// Represents a field used to sort results
    /// </summary>
    public struct SortableField
    {
        /// <summary>
        /// The field name to sort by
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// The way in which the results will be sorted by the field specified.
        /// </summary>
        public SortType SortType { get; }

        /// <summary>
        /// The point to calculate distance from
        /// </summary>
        public ISpatialPoint SpatialPoint { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName">The field name to sort by</param>
        public SortableField(string fieldName)
        {
            FieldName = fieldName;
            SortType = SortType.String;
            SpatialPoint = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName">The field name to sort by</param>
        /// <param name="sortType">The way in which the results will be sorted by the field specified.</param>
        public SortableField(string fieldName, SortType sortType)
        {
            FieldName = fieldName;
            SortType = sortType;
            SpatialPoint = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName">The field name to sort by</param>
        /// <param name="spatialPoint">The point to calculate distance from</param>
        public SortableField(string fieldName, ISpatialPoint spatialPoint)
        {
            FieldName = fieldName;
            SortType = SortType.SpatialDistance;
            SpatialPoint = spatialPoint;
        }
    }
}
