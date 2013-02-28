using System;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Faceting
{
    public interface ISearcherContext
    {
        BaseLuceneSearcher Searcher { get; }

        Searcher LuceneSearcher { get; }

        ReaderDataCollection ReaderData { get; }

        /// <summary>
        /// Reloads the reader data (facets etc.)
        /// </summary>
        void ReloadReaderData();
    }
}