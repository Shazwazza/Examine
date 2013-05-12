using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;

namespace Examine
{
    public interface ISearchResults : IEnumerable<SearchResult>
    {
        int TotalItemCount { get; }
        IEnumerable<SearchResult> Skip(int skip);

        FacetCounts FacetCounts { get; }
    }
}
