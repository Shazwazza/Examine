using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.SearchCriteria
{
    public class SearchOptions
    {        
        public static SearchOptions Default { get; set; }

        static SearchOptions()
        {
            Default = new SearchOptions();
        }

        public int MaxCount { get; set; }

        public bool CountFacets { get; set; }

        public SearchOptions()
        {
            MaxCount = int.MaxValue;
            CountFacets = true;
        }
    }
}
