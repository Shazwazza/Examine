using Examine.Search;

namespace Examine.Search
{
    /// <inheritdoc/>
    public struct ExamineValue : IExamineValue
    {
        /// <inheritdoc/>
        public ExamineValue(Examineness vagueness, string value)
            : this(vagueness, value, 1)
        {
        }

        /// <inheritdoc/>
        public ExamineValue(Examineness vagueness, string value, float level)
        {
            Examineness = vagueness;
            Value = value;
            Level = level;
        }

        /// <inheritdoc/>
        public Examineness Examineness { get; }

        /// <inheritdoc/>
        public string Value { get; }

        /// <inheritdoc/>
        public float Level { get; }

    }
}
