using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Faceting
{
    public class MultiSearcherContext : ISearcherContext
    {
        public Searcher LuceneSearcher { get; private set; }
        public IndexReaderDataCollection ReaderData { get; private set; }

        public BaseLuceneSearcher Searcher { get; private set; }

        public MultiSearcherContext(BaseLuceneSearcher searcher, IEnumerable<ISearcherContext> contexts)
        {
            Searcher = searcher;
            LuceneSearcher = new MultiSearcher(contexts.Select(c=>c.LuceneSearcher).ToArray());
            ReaderData = IndexReaderDataCollection.Join(contexts.Select(c => c.ReaderData), this);
        }
    }
}
