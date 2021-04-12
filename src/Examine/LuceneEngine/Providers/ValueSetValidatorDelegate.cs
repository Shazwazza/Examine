using System;

namespace Examine.LuceneEngine.Providers
{


    /// <summary>
    /// Simple validator that uses a delegate for validation
    /// </summary>
    public class ValueSetValidatorDelegate : IValueSetValidator
    {
        private readonly Func<ValueSet, ValueSetValidationResult> _validator;

        public ValueSetValidatorDelegate(Func<ValueSet, ValueSetValidationResult> validator)
        {
            _validator = validator;
        }

        public ValueSetValidationResult Validate(ValueSet valueSet)
        {
            return _validator(valueSet);
        }
    }
}