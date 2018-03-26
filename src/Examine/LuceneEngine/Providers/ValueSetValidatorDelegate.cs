using System;

namespace Examine.LuceneEngine.Providers
{
    /// <summary>
    /// Simple validator that uses a delegate for validation
    /// </summary>
    public class ValueSetValidatorDelegate : IValueSetValidator
    {
        private readonly Func<ValueSet, bool> _validator;

        public ValueSetValidatorDelegate(Func<ValueSet, bool> validator)
        {
            _validator = validator;
        }

        public bool Validate(ValueSet valueSet)
        {
            return _validator(valueSet);
        }
    }
}