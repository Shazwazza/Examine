using System;

namespace Examine
{
    /// <summary>
    /// Contains the names of field definition types
    /// </summary>
    public static class FieldDefinitionTypes
    {
        /// <summary>
        /// Will be indexed as an integer
        /// </summary>
        public const string Integer = "int";

        /// <summary>
        /// Will be indexed as a float
        /// </summary>
        public const string Float = "float";

        /// <summary>
        /// Will be indexed as a double
        /// </summary>
        public const string Double = "double";

        /// <summary>
        /// Will be indexed as a long
        /// </summary>
        public const string Long = "long";

        /// <summary>
        /// Will be indexed DateTime represented as a long
        /// </summary>
        public const string DateTime = "datetime";

        /// <summary>
        /// Will be indexed DateTime but with precision only to the year represented as a long
        /// </summary>
        public const string DateYear = "date.year";

        /// <summary>
        /// Will be indexed DateTime but with precision only to the month represented as a long
        /// </summary>
        public const string DateMonth = "date.month";

        /// <summary>
        /// Will be indexed DateTime but with precision only to the day represented as a long
        /// </summary>
        public const string DateDay = "date.day";

        /// <summary>
        /// Will be indexed DateTime but with precision only to the hour represented as a long
        /// </summary>
        public const string DateHour = "date.hour";

        /// <summary>
        /// Will be indexed DateTime but with precision only to the minute represented as a long
        /// </summary>
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
        /// Facetable version of <see cref="Integer"/>
        /// </summary>
        public const string FacetTaxonomyInteger = "facettaxonomyint";

        /// <summary>
        /// Facetable version of <see cref="Float"/>
        /// </summary>
        public const string FacetTaxonomyFloat = "facettaxonomyfloat";

        /// <summary>
        /// Facetable version of <see cref="Double"/>
        /// </summary>
        public const string FacetTaxonomyDouble = "facettaxonomydouble";

        /// <summary>
        /// Facetable version of <see cref="Long"/>
        /// </summary>
        public const string FacetTaxonomyLong = "facettaxonomylong";

        /// <summary>
        /// Facetable version of <see cref="DateTime"/>
        /// </summary>
        public const string FacetTaxonomyDateTime = "facettaxonomydatetime";

        /// <summary>
        /// Facetable version of <see cref="DateYear"/>
        /// </summary>
        public const string FacetTaxonomyDateYear = "facettaxonomydate.year";

        /// <summary>
        /// Facetable version of <see cref="DateMonth"/>
        /// </summary>
        public const string FacetTaxonomyDateMonth = "facettaxonomydate.month";

        /// <summary>
        /// Facetable version of <see cref="DateDay"/>
        /// </summary>
        public const string FacetTaxonomyDateDay = "facettaxonomydate.day";

        /// <summary>
        /// Facetable version of <see cref="DateHour"/>
        /// </summary>
        public const string FacetTaxonomyDateHour = "facettaxonomydate.hour";

        /// <summary>
        /// Facetable version of <see cref="DateMinute"/>
        /// </summary>
        public const string FacetTaxonomyDateMinute = "facettaxonomydate.minute";

        /// <summary>
        /// Facetable version of <see cref="FullText"/> stored in the Taxonomy Index
        /// </summary>
        public const string FacetTaxonomyFullText = "facettaxonomyfulltext";

        /// <summary>
        /// Facetable version of <see cref="FullTextSortable"/> stored in the Taxonomy Index
        /// </summary>
        public const string FacetTaxonomyFullTextSortable = "facettaxonomyfulltextsortable";


    }
}
