using System.Collections.Generic;
using Examine.Lucene;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Examine.Test
{
    public class TestIndex : LuceneIndex
    {
        public const string TestIndexName = "testIndexer";

        public TestIndex(ILoggerFactory loggerFactory, IOptionsMonitor<LuceneDirectoryIndexOptions> options)
            : base(loggerFactory, TestIndexName, options)
        {
            RunAsync = false;
        }

        public TestIndex(ILoggerFactory loggerFactory, IOptionsMonitor<LuceneIndexOptions> options, IndexWriter writer)
            : base(loggerFactory, TestIndexName, options, writer)
        {
            RunAsync = false;
        }

        public IEnumerable<ValueSet> AllData()
        {
            var data = new List<ValueSet>();
            for (int i = 0; i < 100; i++)
            {
                data.Add(ValueSet.FromObject(i.ToString(), "category" + (i % 2), new { item1 = "value" + i, item2 = "value" + i }));
            }
            return data;
        }
    }
}
