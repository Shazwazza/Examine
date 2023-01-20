using Examine.Suggest;
using Lucene.Net.Analysis;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Lucene Suggester query time options
    /// </summary>
    public class LuceneSuggestionOptions : SuggestionOptions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="top">Clamp number of results</param>
        /// <param name="suggesterName">The name of the Suggester to use</param>
        /// <param name="analyzer">Query time Analyzer</param>
        /// <param name="suggestionMode">Suggestion Mode</param>
        public LuceneSuggestionOptions(int top = 5, string suggesterName = null, SuggestMode suggestionMode = default) : base(top, suggesterName)
        {
            SuggestionMode = suggestionMode;
        }

        public SuggestMode SuggestionMode { get; }

        /// <summary>
        /// Set of strategies for suggesting related terms
        /// </summary>
        public enum SuggestMode
        {
            /// <summary>
            /// Generate suggestions only for terms not in the index (default)
            /// </summary>
            SUGGEST_WHEN_NOT_IN_INDEX = 0,

            /// <summary>
            /// Return only suggested words that are as frequent or more frequent than the
            /// searched word
            /// </summary>
            SUGGEST_MORE_POPULAR,

            /// <summary>
            /// Always attempt to offer suggestions (however, other parameters may limit
            /// suggestions. For example, see
            /// <see cref="DirectSpellChecker.MaxQueryFrequency"/> ).
            /// </summary>
            SUGGEST_ALWAYS
        }
    }
}
