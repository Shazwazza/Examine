namespace Examine
{
    /// <summary>
    /// Used to validate a value set to be indexed, if validation fails it will not be indexed
    /// </summary>
    public interface IValueSetValidator
    {
        ValueSetValidationResult Validate(ValueSet valueSet);
    }
}