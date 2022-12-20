namespace Examine.Search
{
    /// <summary>
    /// Represents a range over <see cref="double"/> values.
    /// </summary>
    public class DoubleRange
    {
        /// <inheritdoc/>
        public DoubleRange(string label, double min, bool minInclusive, double max, bool maxInclusive)
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
        public string Label { get; set; }

        /// <summary>
        /// Minimum.
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// True if the minimum value is inclusive.
        /// </summary>
        public bool MinInclusive { get; set; }

        /// <summary>
        /// Maximum.
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// True if the maximum value is inclusive.
        /// </summary>
        public bool MaxInclusive { get; set; }
    }
}
