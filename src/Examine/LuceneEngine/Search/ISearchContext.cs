using System.Collections.Generic;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Search
{
    public interface ISearchContext
    {
        Searcher Searcher { get; }
        IIndexValueType GetValueType(string fieldName);
    }
}