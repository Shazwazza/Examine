using Examine.Suggest;

namespace Examine.Lucene.Suggest
{
    public interface ISuggestionExecutionContext
    {
        /// <summary>
        /// Suggestion Options
        /// </summary>
        SuggestionOptions Options { get; }

        /// <summary>
        /// Retrieves a IndexReaderReference for the index the Suggester is for
        /// </summary>
        /// <returns></returns>
        IIndexReaderReference GetIndexReader();
    }
}
