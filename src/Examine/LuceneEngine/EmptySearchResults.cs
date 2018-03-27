using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Examine.LuceneEngine
{
	internal class EmptySearchResults : ISearchResults
	{
        private EmptySearchResults()
        {   
        }

	    public static ISearchResults Instance { get; } = new EmptySearchResults();

        public IEnumerator<SearchResult> GetEnumerator()
		{
			return Enumerable.Empty<SearchResult>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Enumerable.Empty<SearchResult>().GetEnumerator();
		}

		public int TotalItemCount => 0;

	    public IEnumerable<SearchResult> Skip(int skip)
		{
			return Enumerable.Empty<SearchResult>();
		}
	}
}