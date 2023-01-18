using System;
using Examine.Suggest;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Lucene Search Suggestion Query
    /// </summary>
    public class LuceneSuggestionQuery : ISuggestionQuery, ISuggestionExecutor
    {
        private readonly ISuggesterContext _suggesterContext;
        private string _sourceField = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="suggesterContext">Lucene Suggestion Query Context</param>
        /// <param name="options">Query time suggester options</param>
        public LuceneSuggestionQuery(ISuggesterContext suggesterContext, SuggestionOptions options)
        {
            _suggesterContext = suggesterContext;
        }

        /// <inheritdoc/>
        public ISuggestionQuery SourceField(string fieldName)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException(nameof(fieldName));
            }
            _sourceField = fieldName;
            return this;
        }

        /// <inheritdoc/>
        public ISuggestionResults Execute(string searchText, SuggestionOptions options = null)
        {
            var executor = new LuceneSuggesterExecutor(searchText, options, _sourceField, _suggesterContext);
            var results = executor.Execute();
            return results;
        }
    }
}
