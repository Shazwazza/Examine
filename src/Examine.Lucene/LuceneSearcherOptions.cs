using System;
using Lucene.Net.Analysis;
using Lucene.Net.Facet;

namespace Examine.Lucene
{
    /// <summary>
    /// Represents options for configuring a Lucene searcher.
    /// </summary>
    public class LuceneSearcherOptions
    {
        /// <summary>
        /// Gets or sets the search analyzer.
        /// </summary>
        public Analyzer? Analyzer { get; set; }

        /// <summary>
        /// Gets or sets the facet configuration.
        /// </summary>
        public FacetsConfig? FacetConfiguration { get; set; }
    }
}
