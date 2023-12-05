using System;
using Examine.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Options for Searching After. Used for efficent deep paging.
    /// </summary>
    public class SearchAfterOptions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="documentId">The Id of the last document in the previous result set. The search will search after this document</param>
        /// <param name="documentScore"> The Score of the last document in the previous result set. The search will search after this document</param>
        /// <param name="fields">Search fields. Should contain null or J2N.Int</param>
        /// <param name="shardIndex">The index of the shard the doc belongs to</param>
        public SearchAfterOptions(int documentId, float documentScore, object[]? fields, int shardIndex)
        {
            DocumentId = documentId;
            DocumentScore = documentScore;
            Fields = fields;
            ShardIndex = shardIndex;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="searchAfter">Search After</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public SearchAfterOptions(SearchAfter searchAfter)
        {
            if (searchAfter is null)
            {
                throw new ArgumentNullException(nameof(searchAfter));
            }
            if (string.IsNullOrWhiteSpace(searchAfter.Value))
            {
                throw new ArgumentException(nameof(searchAfter));
            }
            var searchAfterVals = searchAfter.Value.Split('|');
            DocumentId = int.Parse(searchAfterVals[0]);
            DocumentScore = int.Parse(searchAfterVals[1]);
            if (!string.IsNullOrEmpty(searchAfterVals[2]))
            {
                ShardIndex = int.Parse(searchAfterVals[2]);
            }
        }

        /// <summary>
        /// The Id of the last document in the previous result set.
        /// The search will search after this document
        /// </summary>
        public int DocumentId { get; }

        /// <summary>
        /// The Score of the last document in the previous result set.
        /// The search will search after this document
        /// </summary>
        public float DocumentScore { get; }

        /// <summary>
        /// The index of the shard the doc belongs to
        /// </summary>
        public int? ShardIndex { get; }

        /// <summary>
        /// Search fields. Should contain null or J2N.Int
        /// </summary>
        public object[]? Fields { get; }

        /// <summary>
        /// To Search After
        /// </summary>
        /// <returns></returns>
        public SearchAfter ToSearchAfter()
        {
            var searchAfterSerialized = string.Join("|", DocumentId, DocumentScore, ShardIndex);
            return new SearchAfter(searchAfterSerialized);
        }
    }
}
