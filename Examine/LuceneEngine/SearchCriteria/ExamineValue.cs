using Examine.SearchCriteria;

namespace Examine.LuceneEngine.SearchCriteria
{
    internal class ExamineValue : IExamineValue
    {
        public ExamineValue(Examineness vagueness, string value)
            : this(vagueness, value, 1)
        {
        }

        public ExamineValue(Examineness vagueness, string value, float level)
        {
            //remove the stop words
            //var realVal = value.RemoveStopWords();

            this.Examineness = vagueness;
            this.Value = value;
            this.Level = level;
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

        public float Level
        {
            get;
            private set;
        }

    }
}
