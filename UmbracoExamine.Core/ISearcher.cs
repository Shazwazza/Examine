using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoExamine.Core.SearchCriteria;

namespace UmbracoExamine.Core
{
    public interface ISearcher
    {

        IEnumerable<SearchResult> Search(string searchText, int maxResults, bool useWildcards);
        IEnumerable<SearchResult> Search(ISearchCriteria searchParameters);

        ISearchCriteria CreateSearchCriteria(int maxResults, IndexType type);
    }
}
