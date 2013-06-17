using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.SearchCriteria;

namespace Examine
{
    public interface ISearchResults : IEnumerable<SearchResult>
    {
        int TotalItemCount { get; }
        IEnumerable<SearchResult> Skip(int skip);

        FacetCounts FacetCounts { get; }


        IDictionary<string, List<Func<SearchResult,string>>> Highlighters { get; }

        ICriteriaContext CriteriaContext { get; }
    }
}
