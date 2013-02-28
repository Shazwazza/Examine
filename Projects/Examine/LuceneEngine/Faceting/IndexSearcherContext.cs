using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Faceting
{
    public class IndexSearcherContext : ISearcherContext
    {
        public IndexSearcher LuceneSearcher { get; private set; }

        public BaseLuceneSearcher Searcher { get; private set; }


        public ReaderDataCollection ReaderData { get; set; }        

        public IndexSearcherContext(BaseLuceneSearcher searcher, IndexSearcher luceneSearcher, ReaderDataCollection oldData = null)
        {
            Searcher = searcher;
            LuceneSearcher = luceneSearcher;
            ReaderData = ReaderDataCollection.FromReader(luceneSearcher.GetIndexReader(), this, oldData);
        }

        Searcher ISearcherContext.LuceneSearcher { get { return LuceneSearcher; } }

        public void ReloadReaderData()
        {
            ReaderData = ReaderDataCollection.FromReader(LuceneSearcher.GetIndexReader(), this, null);   
        }
    }
}
