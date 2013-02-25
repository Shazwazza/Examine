using System;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Faceting
{
    public interface ISearcherContext
    {
        BaseLuceneSearcher Searcher { get; }

        Searcher LuceneSearcher { get; }

        IndexReaderDataCollection ReaderData { get; }        
    }
}