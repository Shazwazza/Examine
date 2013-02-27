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
        
        public ISearcherContext[] Contexts { get; private set; }

        public MultiSearcherContext(BaseLuceneSearcher searcher, IEnumerable<ISearcherContext> contexts)
        {
            Searcher = searcher;
            Contexts = contexts.ToArray();
            LuceneSearcher = new MultiSearcher(Contexts.Select(c=>c.LuceneSearcher).ToArray());
            ReaderData = IndexReaderDataCollection.Join(Contexts.Select(c => c.ReaderData), this);
        }

        public void ReloadReaderData()
        {
            ReaderData = IndexReaderDataCollection.Join(Contexts.Select(c => c.ReaderData), this);
        }
    }
}
