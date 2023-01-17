using System;
using System.Collections.Generic;

namespace Examine.Suggest
{
    public abstract class BaseSuggesterProvider : ISuggester
    {
        protected BaseSuggesterProvider(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            Name = name;
        }
        public string Name { get; }

        public abstract ISuggestionQuery CreateSuggestionQuery();
        public abstract ISuggestionResults Suggest(string searchText, string sourceFieldName, SuggestionOptions options = null);
    }
}
