namespace Examine
{
    /// <summary>
    /// Constant names for speciffic fields
    /// </summary>
    public static class ExamineFieldNames
    {
        /// <summary>
        /// The prefix characters denoting a special field stored in the lucene index for use internally
        /// </summary>
        public const string SpecialFieldPrefix = "__";

        /// <summary>
        /// The prefix added to a field when it is included in the index for sorting
        /// </summary>
        public const string SortedFieldNamePrefix = "__Sort_";

        /// <summary>
        /// Used to store a non-tokenized key for the document for the Category
        /// </summary>
        public const string CategoryFieldName = "__IndexType";

        /// <summary>
        /// Used to store a non-tokenized type for the document
        /// </summary>
        public const string ItemIdFieldName = "__NodeId";

        /// <summary>
        /// Used to store the item type for a document
        /// </summary>
        public const string ItemTypeFieldName = "__NodeTypeAlias";

        /// <summary>
        /// The default field name for storing facet information
        /// </summary>
        public const string DefaultFacetsName = "$facets";
    }

    
}

