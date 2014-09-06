using System;
using System.Collections.Generic;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.SearchCriteria;

namespace Examine.LuceneEngine
{
    public interface ILuceneSearchResults : ISearchResults<LuceneSearchResult>
    {
        FacetCounts FacetCounts { get; }

        IDictionary<string, List<Func<LuceneSearchResult, string>>> Highlighters { get; }

        ICriteriaContext CriteriaContext { get; }

        IEnumerable<LuceneSearchResult> Skip(int skip);
    }
}