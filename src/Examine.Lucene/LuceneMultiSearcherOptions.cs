using System;

namespace Examine.Lucene
{
    /// <summary>
    /// Examine Lucene MultiSearcher Configuration
    /// </summary>
    public class LuceneMultiSearcherOptions : LuceneSearcherOptions
    {
        /// <summary>
        /// Index Names to search
        /// </summary>
        public string[] IndexNames { get; set; } = Array.Empty<string>();
    }
}
