namespace Examine.Search
{
    /// <summary>
    /// Term
    /// </summary>
    public struct FilterTerm
    {
        /// <summary>
        /// Name of the Field
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// Value of the Term
        /// </summary>
        public string FieldValue { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName">Name of the Field</param>
        /// <param name="fieldValue">Value of the Term</param>
        public FilterTerm(string fieldName, string fieldValue)
        {
            FieldName = fieldName;
            FieldValue = fieldValue;
        }

    }
}
