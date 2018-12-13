using System.Collections.Generic;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.SearchCriteria
{
    public interface ICriteriaContext
    {
        Searcher Searcher { get; }
        IEnumerable<IIndexValueType> ValueTypes { get; }
        IIndexValueType GetValueType(string fieldName);
    }
}