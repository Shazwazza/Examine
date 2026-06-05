namespace Examine
{
    /// <summary>
    /// Used to validate a value set to be indexed, if validation fails it will not be indexed
    /// </summary>
    public interface IValueSetValidator
    {
        /// <summary>
        /// Validates the value set
        /// </summary>
        /// <param name="valueSet"></param>
        /// <returns></returns>
        ValueSetValidationResult Validate(ValueSet valueSet);
    }
}
