using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Lucene.Search
{
    public class LuceneSearchResult : SearchResult, ISearchResult
    {
        public LuceneSearchResult(string id, float score, Func<IDictionary<string, List<string>>> lazyFieldVals, int shardId)
            : base(id, score, lazyFieldVals)
        {
            ShardIndex = shardId;
        }

        public int ShardIndex { get; }
    }
}
