namespace Examine
{
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

        public const string ItemTypeFieldName = "__NodeTypeAlias";
    }

    
}

