using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Examine
{
	public sealed class EmptySearchResults : ISearchResults
	{
        private EmptySearchResults()
        {   
        }

	    public static ISearchResults Instance { get; } = new EmptySearchResults();

        public IEnumerator<ISearchResult> GetEnumerator()
		{
			return Enumerable.Empty<ISearchResult>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Enumerable.Empty<ISearchResult>().GetEnumerator();
		}

		public long TotalItemCount => 0;


        public IEnumerable<ISearchResult> Skip(int skip)
		{
			return Enumerable.Empty<ISearchResult>();
		}

        public IEnumerable<ISearchResult> SkipTake(int skip, int? take = null)
        {
			return Enumerable.Empty<ISearchResult>();
		}
    }
}
