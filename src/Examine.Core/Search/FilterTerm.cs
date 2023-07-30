namespace Examine.Search
{
    public struct FilterTerm
    {
        public string FieldName { get; }

        public FilterTerm(string fieldName, string fieldValue)
        {
            FieldName = fieldName;
            FieldValue = fieldValue;
        }

        public string FieldValue { get;  }
    }
}
