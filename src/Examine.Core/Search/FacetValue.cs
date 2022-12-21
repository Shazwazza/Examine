namespace Examine.Search
{
    public readonly struct FacetValue : IFacetValue
    {
        public string Label { get; }

        public float Value { get; }

        public FacetValue(string label, float value)
        {
            Label = label;
            Value = value;
        }
    }
}
