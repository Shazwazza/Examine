using System;
using System.Collections.Generic;
using Examine.Suggest;
using Lucene.Net.Analysis;

namespace Examine.Lucene.Suggest
{
    public abstract class BaseLuceneSuggester : BaseSuggesterProvider
    {
        private readonly Analyzer _suggestionSearchAnalyzer;

        protected BaseLuceneSuggester(string name, Analyzer suggestionSearchAnalyzer = null) : base(name)
        {
            _suggestionSearchAnalyzer = suggestionSearchAnalyzer;
        }

        public Analyzer SuggestionSearchAnalyzer => _suggestionSearchAnalyzer;


        public abstract ISuggesterContext GetSuggesterContext();

        public override ISuggestionQuery CreateSuggestionQuery()
        {
            return CreateSuggestionQuery(new SuggestionOptions());
        }

        public virtual ISuggestionQuery CreateSuggestionQuery(SuggestionOptions options = null)
        {
            return new LuceneSuggestionQuery(GetSuggesterContext(), options);
        }
        public override ISuggestionResults Suggest(string searchText, ISet<string> sourceFieldNames, SuggestionOptions options = null)
        {
            var suggestionExecutor = CreateSuggestionQuery().SourceFields(sourceFieldNames);
            return suggestionExecutor.Execute(searchText, options);
        }
    }
}
