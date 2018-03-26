namespace Examine.LuceneEngine.Providers
{
    /// <summary>
    /// Used to validate a value set to be indexed, if validation fails it will not be indexed
    /// </summary>
    public interface IValueSetValidator
    {
        bool Validate(ValueSet valueSet);
    }
}