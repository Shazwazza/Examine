using Lucene.Net.Util;

namespace Examine
{
    /// <summary>
    /// Information about lucene
    /// </summary>
    public static class LuceneInfo
    {
        /// <summary>
        /// Gets the current lucene version
        /// </summary>
        public static LuceneVersion CurrentVersion { get; } = LuceneVersion.LUCENE_48;
    }
}
