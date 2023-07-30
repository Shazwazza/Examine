namespace Examine.Search
{
    /// <inheritdoc/>
    public readonly struct FacetValue : IFacetValue
    {
        /// <inheritdoc/>
        public string Label { get; }

        /// <inheritdoc/>
        public float Value { get; }

        /// <inheritdoc/>
        public FacetValue(string label, float value)
        {
            Label = label;
            Value = value;
        }
    }
}
