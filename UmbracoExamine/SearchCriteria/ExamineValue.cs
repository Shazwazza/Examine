using Examine.SearchCriteria;

namespace UmbracoExamine.SearchCriteria
{
    internal class ExamineValue : IExamineValue
    {
        public ExamineValue(Examineness vagueness, string value)
        {
            this.Examineness = vagueness;
            this.Value = value;
        }

        public Examineness Examineness
        {
            get;
            private set;
        }

        public string Value
        {
            get;
            private set;
        }

        internal double Scope
        {
            get;
            set;
        }
    }
}
