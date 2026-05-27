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


#pragma warning disable IDE0060 // Remove unused parameter
        public IEnumerable<ISearchResult> Skip(int skip)
#pragma warning restore IDE0060 // Remove unused parameter
        {
			return Enumerable.Empty<ISearchResult>();
		}

#pragma warning disable IDE0060 // Remove unused parameter
        public IEnumerable<ISearchResult> SkipTake(int skip, int? take = null)
#pragma warning restore IDE0060 // Remove unused parameter
        {
			return Enumerable.Empty<ISearchResult>();
		}
    }
}
