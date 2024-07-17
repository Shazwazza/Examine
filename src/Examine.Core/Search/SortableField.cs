namespace Examine.Search
{
    /// <summary>
    /// Represents a field used to sort results
    /// </summary>
    public readonly struct SortableField
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
        /// Constructor
        /// </summary>
        /// <param name="fieldName"></param>
        public SortableField(string fieldName)
        {
            FieldName = fieldName;
            SortType = SortType.String;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="sortType"></param>
        public SortableField(string fieldName, SortType sortType)
        {
            FieldName = fieldName;
            SortType = sortType;
        }
    }
}
