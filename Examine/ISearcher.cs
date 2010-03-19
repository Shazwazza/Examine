using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.SearchCriteria;

namespace Examine
{
    public interface ISearcher
    {

        IEnumerable<SearchResult> Search(string searchText, int maxResults, bool useWildcards);
        IEnumerable<SearchResult> Search(ISearchCriteria searchParameters);

        ISearchCriteria CreateSearchCriteria(int maxResults, IndexType type);
    }
}
