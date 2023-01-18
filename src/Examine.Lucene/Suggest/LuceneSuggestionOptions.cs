using Examine.Suggest;
using Lucene.Net.Analysis;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Lucene Suggester query qime options
    /// </summary>
    public class LuceneSuggestionOptions : SuggestionOptions
    {
        /// <summary>
        /// Contstuctor
        /// </summary>
        /// <param name="top">Clamp number of results</param>
        /// <param name="suggesterName">The name of the Suggester to use</param>
        /// <param name="analyzer">Query time Analyzer</param>
        public LuceneSuggestionOptions(int top = 5, string suggesterName = null, Analyzer analyzer = null) : base(top, suggesterName)
        {
            Analyzer = analyzer;
        }

        /// <summary>
        /// Query Time Analyzer
        /// </summary>
        public Analyzer Analyzer { get; }


    }
}
