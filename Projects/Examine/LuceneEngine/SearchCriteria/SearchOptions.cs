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
        /// Constructor
        /// </summary>
        public SearchOptions()
        {
            //MaxCount = int.MaxValue;
            CountFacets = false;
            CountFacetReferences = false;
        }

        public int MaxCount { get; set; }

        public bool CountFacets { get; set; }

        public bool CountFacetReferences { get; set; }

        public FacetCounts FacetReferenceCountBasis { get; set; }

        
    }
}
