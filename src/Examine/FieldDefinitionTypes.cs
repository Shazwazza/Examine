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
        public const string Raw = "raw";

        /// <summary>
        /// This is the default field type
        /// </summary>
        public const string FullText = "fulltext";
        public const string FullTextSortable = "fulltextsortable";

    }
}