namespace Examine.Search
{
    /// <summary>
    /// Represents a range over <see cref="long"/> values.
    /// </summary>
    public class Int64Range
    {
        /// <inheritdoc/>
        public Int64Range(string label, long min, bool minInclusive, long max, bool maxInclusive)
        {
            Label = label;
            Min = min;
            MinInclusive = minInclusive;
            Max = max;
            MaxInclusive = maxInclusive;
        }

        /// <summary>
        /// Label that identifies this range.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Minimum.
        /// </summary>
        public long Min { get; }

        /// <summary>
        /// True if the minimum value is inclusive.
        /// </summary>
        public bool MinInclusive { get; }

        /// <summary>
        /// Maximum.
        /// </summary>
        public long Max { get; }

        /// <summary>
        /// True if the maximum value is inclusive.
        /// </summary>
        public bool MaxInclusive { get; }
    }
}
