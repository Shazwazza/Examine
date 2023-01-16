namespace Examine
{
    /// <summary>
    /// Contains the names of field definition types
    /// </summary>
    public static class FieldDefinitionTypes
    {
        public const string Integer = "int";
        public const string Float = "float";
        public const string Double = "double";
        public const string Long = "long";
        public const string DateTime = "datetime";
        public const string DateYear = "date.year";
        public const string DateMonth = "date.month";
        public const string DateDay = "date.day";
        public const string DateHour = "date.hour";
        public const string DateMinute = "date.minute";

        /// <summary>
        /// Will be indexed without analysis
        /// </summary>
        public const string Raw = "raw";

        /// <summary>
        /// The default type, will be indexed with the specified indexer's analyzer
        /// </summary>
        public const string FullText = "fulltext";

        /// <summary>
        /// Will be indexed with the specified indexer's analyzer and with a sorting field
        /// </summary>
        public const string FullTextSortable = "fulltextsortable";

        /// <summary>
        /// Will be indexed with a culture invariant whitespace analyzer, this is what is used for 'special' prefixed fields
        /// </summary>
        public const string InvariantCultureIgnoreCase = "invariantcultureignorecase";

        /// <summary>
        /// Will be indexed with an email address analyzer
        /// </summary>
        public const string EmailAddress = "emailaddress";

        /// <summary>
        /// The default type, will be indexed with the specified indexer's analyzer and the AnalyzingSuggester Lookup
        /// </summary>
        public const string FullTextSuggestable = "fulltextsuggestable";

        /// <summary>
        /// Will be indexed with the specified indexer's analyzer and with a sorting field and the AnalyzingSuggester Lookup
        /// </summary>
        public const string FullTextSortableSuggestable = "fulltextsortablesuggestable";

        /// <summary>
        /// The default type, will be indexed with the specified indexer's analyzer and the JaspellLookup Lookup
        /// </summary>
        public const string FullTextSpelling = "fulltextspelling";

        /// <summary>
        /// Will be indexed with the specified indexer's analyzer and with a sorting field and the JaspellLookup Lookup
        /// </summary>
        public const string FullTextSortableSpelling = "fulltextsortablespelling";

        /// <summary>
        /// The default type, will be indexed with the specified indexer's analyzer and the FuzzySuggester Lookup
        /// </summary>
        public const string FullTextFuzzySuggestable = "fulltextfuzzysuggestable";

        /// <summary>
        /// Will be indexed with the specified indexer's analyzer and with a sorting field and the FuzzySuggester Lookup
        /// </summary>
        public const string FullTextSortableFuzzySuggestable = "fulltextsortablesfuzzyuggestable";

    }
}
