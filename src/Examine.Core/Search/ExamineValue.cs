using Examine.Search;

namespace Examine.Search
{
    public readonly struct ExamineValue : IExamineValue
    {
        public ExamineValue(Examineness vagueness, string value)
            : this(vagueness, value, 1)
        {
        }

        public ExamineValue(Examineness vagueness, string value, float level)
        {
            Examineness = vagueness;
            Value = value;
            Level = level;
        }

        public Examineness Examineness { get; }

        public string Value { get; }

        public float Level { get; }
    }
}
