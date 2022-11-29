namespace Examine
{
    public struct ValueSetValidationResult
    {
        public ValueSetValidationResult(ValueSetValidationStatus status, ValueSet valueSet)
        {
            Status = status;
            ValueSet = valueSet;
        }

        public ValueSetValidationStatus Status { get; }

        public ValueSet ValueSet { get; }
    }
}
