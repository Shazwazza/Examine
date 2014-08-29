using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;

namespace Examine.LuceneEngine.SearchCriteria
{
    /// <summary>
    /// Options for Lucene search
    /// </summary>
    internal class SearchOptions
    {        
        /// <summary>
        /// The default options
        /// </summary>
        public static SearchOptions Default { get; private set; }

        static SearchOptions()
        {
            Default = new SearchOptions();
        }

        public int MaxCount { get; set; }

        public bool CountFacets { get; set; }

        public bool CountFacetReferences { get; set; }

        public FacetCounts FacetReferenceCountBasis { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SearchOptions()
        {
            //MaxCount = int.MaxValue;
            CountFacets = true;
        }
    }
}
