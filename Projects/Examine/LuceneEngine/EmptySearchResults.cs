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

	    //public IEnumerator<ISearchResult> GetEnumerator()
	    IEnumerator<LuceneSearchResult> IEnumerable<LuceneSearchResult>.GetEnumerator()
	    {
            return Enumerable.Empty<LuceneSearchResult>().GetEnumerator();
	    }

	    public IEnumerator<SearchResult> GetEnumerator()
	    {
            //return Enumerable.Empty<ISearchResult>().GetEnumerator();
            return Enumerable.Empty<SearchResult>().GetEnumerator();
	    }

	    IEnumerator IEnumerable.GetEnumerator()
		{
			return Enumerable.Empty<SearchResult>().GetEnumerator();
		}

		public int TotalItemCount
		{
			get { return 0; }
		}

	    IEnumerable<LuceneSearchResult> ILuceneSearchResults.Skip(int skip)
	    {
            return Enumerable.Empty<LuceneSearchResult>();
	    }

	    public IEnumerable<SearchResult> Skip(int skip)
		{
			return Enumerable.Empty<SearchResult>();
		}

        public IDictionary<string, List<Func<LuceneSearchResult, string>>> Highlighters { get; private set; }

	    public FacetCounts FacetCounts { get; private set; }
	    

	    public ICriteriaContext CriteriaContext { get; private set; }
	}
}