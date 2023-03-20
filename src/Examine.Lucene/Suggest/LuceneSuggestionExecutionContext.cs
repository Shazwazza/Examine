using Examine.Suggest;

namespace Examine.Lucene.Suggest
{
    internal class LuceneSuggestionExecutionContext : ISuggestionExecutionContext
    {
        private readonly ISuggesterContext _suggesterContext;

        public LuceneSuggestionExecutionContext(SuggestionOptions options, ISuggesterContext suggesterContext)
        {
            Options = options;
            _suggesterContext = suggesterContext;
        }
        public SuggestionOptions Options { get; }

        public IIndexReaderReference GetIndexReader() => _suggesterContext.GetIndexReader();
    }
}
