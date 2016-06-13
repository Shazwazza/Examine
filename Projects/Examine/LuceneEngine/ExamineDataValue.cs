namespace Examine.LuceneEngine
{
    public class FieldOperation
    {
        public FieldOperation(bool analyze, bool store)
        {
            Analyze = analyze;
            Store = store;
        }

        public bool Analyze { get; }
        public bool Store { get; }
    }

    public class ExamineDataValue
    {
        public string FieldName { get; set; }

        public string Value { get; set; }

        public FieldOperation FieldOperation { get; }

        public ExamineDataValue(string fieldName, string value, bool analyze, bool store)
        {
            FieldName = fieldName;
            Value = value;
            FieldOperation = new FieldOperation(analyze, store);
        }
    }
}
