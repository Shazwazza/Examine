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
        /// Initializes a new instance of the <see cref="LuceneSearcherOptions"/> class.
        /// </summary>
        /// <param name="name">The name of the searcher.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        public LuceneSearcherOptions(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Gets or sets the search analyzer.
        /// </summary>
        public Analyzer? Analyzer { get; set; }

        /// <summary>
        /// Gets or sets the facet configuration.
        /// </summary>
        public FacetsConfig? FacetConfiguration { get; set; }

        /// <summary>
        /// Gets the name of the searcher.
        /// </summary>
        public string Name { get; }
    }
}
