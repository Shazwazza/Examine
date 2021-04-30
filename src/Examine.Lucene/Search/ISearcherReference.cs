using System;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    // Dispose will release it from the manager
    public interface ISearcherReference : IDisposable
    {
        IndexSearcher IndexSearcher { get; }
    }
}
