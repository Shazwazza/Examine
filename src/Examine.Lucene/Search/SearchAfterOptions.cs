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
        public SearchAfterOptions(int documentId, float documentScore, object[] fields, int shardIndex)
        {
            DocumentId = documentId;
            DocumentScore = documentScore;
            Fields = fields;
            ShardIndex = shardIndex;
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
    }
}
