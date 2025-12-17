namespace Examine
{
    /// <summary>
    /// Represents a value set validation result
    /// </summary>
    public readonly struct ValueSetValidationResult
    {
        /// <inheritdoc/>
        public ValueSetValidationResult(ValueSetValidationStatus status, ValueSet valueSet)
        {
            Status = status;
            ValueSet = valueSet;
        }

        /// <summary>
        /// The status of the validation
        /// </summary>
        public ValueSetValidationStatus Status { get; }

        /// <summary>
        /// The value set of the validation
        /// </summary>
        public ValueSet ValueSet { get; }
    }
}
