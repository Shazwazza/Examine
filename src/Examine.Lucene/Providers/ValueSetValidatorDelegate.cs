using System;

namespace Examine.Lucene.Providers
{
    /// <summary>
    /// Simple validator that uses a delegate for validation
    /// </summary>
    public class ValueSetValidatorDelegate : IValueSetValidator
    {
        private readonly Func<ValueSet, ValueSetValidationResult> _validator;

        /// <inheritdoc/>
        public ValueSetValidatorDelegate(Func<ValueSet, ValueSetValidationResult> validator)
            => _validator = validator;

        /// <inheritdoc/>
        public ValueSetValidationResult Validate(ValueSet valueSet)
            => _validator(valueSet);
    }
}
