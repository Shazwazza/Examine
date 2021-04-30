using System.Collections.Generic;
using Examine.Lucene.Indexing;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    public interface ISearchContext
    {
        IndexSearcher Searcher { get; }
        IIndexFieldValueType GetFieldValueType(string fieldName);
    }
}