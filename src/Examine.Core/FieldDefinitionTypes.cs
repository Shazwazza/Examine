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
        /// Facetable version of <see cref="Integer"/>
        /// </summary>
        public const string FacetInteger = "facetint";

        /// <summary>
        /// Facetable version of <see cref="Float"/>
        /// </summary>
        public const string FacetFloat = "facetfloat";

        /// <summary>
        /// Facetable version of <see cref="Double"/>
        /// </summary>
        public const string FacetDouble = "facetdouble";

        /// <summary>
        /// Facetable version of <see cref="Long"/>
        /// </summary>
        public const string FacetLong = "facetlong";

        /// <summary>
        /// Facetable version of <see cref="DateTime"/>
        /// </summary>
        public const string FacetDateTime = "facetdatetime";

        /// <summary>
        /// Facetable version of <see cref="DateYear"/>
        /// </summary>
        public const string FacetDateYear = "facetdate.year";

        /// <summary>
        /// Facetable version of <see cref="DateMonth"/>
        /// </summary>
        public const string FacetDateMonth = "facetdate.month";

        /// <summary>
        /// Facetable version of <see cref="DateDay"/>
        /// </summary>
        public const string FacetDateDay = "facetdate.day";

        /// <summary>
        /// Facetable version of <see cref="DateHour"/>
        /// </summary>
        public const string FacetDateHour = "facetdate.hour";

        /// <summary>
        /// Facetable version of <see cref="DateMinute"/>
        /// </summary>
        public const string FacetDateMinute = "facetdate.minute";

        /// <summary>
        /// Facetable version of <see cref="FullText"/>
        /// </summary>
        public const string FacetFullText = "facetfulltext";

        /// <summary>
        /// Facetable version of <see cref="FullTextSortable"/>
        /// </summary>
        public const string FacetFullTextSortable = "facetfulltextsortable";

        /// <summary>
        /// Taxonomy Facetable version of path
        /// </summary>
        public const string TaxonomyFacet = "taxonomyfacet";

    }
}
