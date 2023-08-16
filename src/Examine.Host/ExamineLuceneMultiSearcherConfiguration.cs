
using Lucene.Net.Analysis;
using Lucene.Net.Facet;

namespace Examine
{
    /// <summary>
    /// Examine Lucene MultiSearcher Configuration
    /// </summary>
    public class ExamineLuceneMultiSearcherConfiguration
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Searcher Name</param>
        /// <param name="indexNames">Index Names to search</param>
        public ExamineLuceneMultiSearcherConfiguration(string name, string[] indexNames)
        {
            Name = name;
            IndexNames = indexNames;
        }

        /// <summary>
        /// Searcher Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Index Names to search
        /// </summary>
        public string[] IndexNames { get; }

        /// <summary>
        /// Facet Configuration
        /// </summary>
        public FacetsConfig? FacetConfiguration { get; set; }

        /// <summary>
        /// Search Analyzer
        /// </summary>
        public Analyzer? Analyzer { get; set; }
    }
}
