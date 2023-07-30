using System;

namespace Examine.Suggest
{
    /// <inheritdoc/>
    public abstract class BaseSuggesterProvider : ISuggester
    {
        /// <inheritdoc/>
        protected BaseSuggesterProvider(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            Name = name;
        }
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public abstract ISuggestionQuery CreateSuggestionQuery();

        /// <inheritdoc/>
        public abstract ISuggestionResults Suggest(string searchText, SuggestionOptions? options = null);
    }
}
