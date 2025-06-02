using Lucene.Net.Analysis;
using Lucene.Net.Facet;

namespace Examine.Lucene
{
    /// <summary>
    /// Examine Lucene MultiSearcher Configuration
    /// </summary>
    public class LuceneMultiSearcherOptions : LuceneSearcherOptions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Searcher Name</param>
        /// <param name="indexNames">Index Names to search</param>
        public LuceneMultiSearcherOptions(string name, string[] indexNames)
            : base(name)
        {
            IndexNames = indexNames;
        }

        /// <summary>
        /// Index Names to search
        /// </summary>
        public string[] IndexNames { get; }
    }
}
