namespace Examine.Search
{
    /// <summary>
    /// Represents a range over <see cref="float"/> values.
    /// </summary>
    public readonly struct FloatRange
    {
        /// <inheritdoc/>
        public FloatRange(string label, float min, bool minInclusive, float max, bool maxInclusive)
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
        public float Min { get; }

        /// <summary>
        /// True if the minimum value is inclusive.
        /// </summary>
        public bool MinInclusive { get; }

        /// <summary>
        /// Maximum.
        /// </summary>
        public float Max { get; }

        /// <summary>
        /// True if the maximum value is inclusive.
        /// </summary>
        public bool MaxInclusive { get; }
    }
}
