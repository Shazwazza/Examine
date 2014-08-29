using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.SearchCriteria;

namespace Examine.LuceneEngine
{
	internal class EmptySearchResults : ILuceneSearchResults
	{
		public int TotalItemCount
		{
			get { return 0; }
		}

        public IEnumerable<LuceneSearchResult> Skip(int skip)
		{
            return Enumerable.Empty<LuceneSearchResult>();
		}

        public IDictionary<string, List<Func<LuceneSearchResult, string>>> Highlighters { get; private set; }

	    public FacetCounts FacetCounts { get; private set; }
	    

	    public ICriteriaContext CriteriaContext { get; private set; }

        public IEnumerator<LuceneSearchResult> GetEnumerator()
        {
            return Enumerable.Empty<LuceneSearchResult>().GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
	    {
	        return GetEnumerator();
	    }
	}
}