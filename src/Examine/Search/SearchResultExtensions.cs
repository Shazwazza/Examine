using System;
using System.Collections.Generic;

namespace Examine.Search
{
    public static class SearchResultExtensions
    {
        
        public static IEnumerable<ISearchResult> SkipTake(this ISearchResults searchResults, int skip, int? take = null)
        {
            if (!(searchResults is ISearchResults2 results2))
            {
                throw new NotSupportedException("ISearchResults2 is not implemented");
            }
            return results2.SkipTake(skip, take);
        }
    }
}
