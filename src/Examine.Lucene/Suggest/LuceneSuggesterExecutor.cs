using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Suggest;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search.Spell;
using Lucene.Net.Search.Suggest.Analyzing;
using static Lucene.Net.Search.Suggest.Lookup;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Suggester Executor for a Lucene Index
    /// </summary>
    internal class LuceneSuggesterExecutor
    {
        private readonly string _searchText;
        private readonly SuggestionOptions _options;
        private readonly ISuggesterContext _suggesterContext;
        private readonly ISuggestionResults _emptySuggestionResults = new LuceneSuggestionResults(Array.Empty<ISuggestionResult>());

        public LuceneSuggesterExecutor(string searchText, SuggestionOptions options, ISuggesterContext suggesterContext)
        {
            _searchText = searchText;
            _options = options;
            _suggesterContext = suggesterContext;
        }

        /// <summary>
        /// Execute the Suggester
        /// </summary>
        /// <returns>Suggestion Results</returns>
        public ISuggestionResults Execute()
        {
            if (_options.SuggesterName == null)
            {
                return _emptySuggestionResults;
            }

            var suggesters = _suggesterContext.GetSuggesterDefinitions();
            var suggester = suggesters.FirstOrDefault(x => x.Name == _options.SuggesterName);
            if (suggester.Name == null || suggester.SourceFields == null)
            {
                return _emptySuggestionResults;
            }
            var luceneSuggesterDefinition = suggester as LuceneSuggesterDefinition;
            if(luceneSuggesterDefinition == null)
            {
                return _emptySuggestionResults;
            }
            ISuggestionExecutionContext ctx = new LuceneSuggestionExecutionContext(_options,_suggesterContext);
            return luceneSuggesterDefinition.ExecuteSuggester(_searchText, ctx);
        }
    }
}
