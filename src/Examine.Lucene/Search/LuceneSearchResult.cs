using System;
using System.Collections.Generic;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Lucene Index Search Result
    /// </summary>
    public class LuceneSearchResult : SearchResult, ISearchResult
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LuceneSearchResult(string id, float score, Func<IDictionary<string, List<string>>> lazyFieldVals, int shardId)
            : base(id, score, lazyFieldVals)
        {
            ShardIndex = shardId;
        }

        /// <summary>
        /// Index Shard Id
        /// </summary>
        public int ShardIndex { get; }
    }
}
