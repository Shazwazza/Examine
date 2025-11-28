using System;
using Examine.Search;

namespace Examine.Search
{
    /// <inheritdoc/>
    public sealed class ExamineValue : IExamineValue
    {
        /// <inheritdoc/>
        [Obsolete("Use ExamineValue.Create instead")]
        public ExamineValue(Examineness vagueness, string value)
            : this(vagueness, value, 1)
        {
        }

        /// <inheritdoc/>
        [Obsolete("Use ExamineValue.Create instead")]
        public ExamineValue(Examineness vagueness, string value, float level)
        {
            Examineness = vagueness;
            Value = value;
            Level = level;
        }

        /// <summary>
        /// Creates an examine value
        /// </summary>
        public static IExamineValue Create(Examineness vagueness, string value)
            => new ExamineValueRecord(value, vagueness, 1);

        /// <summary>
        /// Creates an examine value
        /// </summary>
        public static IExamineValue Create(Examineness vagueness, string value, float level)
            => new ExamineValueRecord(value, vagueness, level);

        /// <inheritdoc/>
        public Examineness Examineness { get; }

        /// <inheritdoc/>
        public string Value { get; }

        /// <inheritdoc/>
        public float Level { get; }

        private readonly record struct ExamineValueRecord(string Value, Examineness Examineness, float Level) : IExamineValue;
    }
}
