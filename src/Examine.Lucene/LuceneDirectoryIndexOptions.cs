
using Examine.Lucene.Directories;
using Lucene.Net.Store;

namespace Examine.Lucene
{
    /// <summary>
    /// Represents options for the lucene index
    /// </summary>
    public class LuceneDirectoryIndexOptions : LuceneIndexOptions
    {
        /// <summary>
        /// Returns the directory factory to use
        /// </summary>
        public IDirectoryFactory? DirectoryFactory { get; set; }

        /// <summary>
        /// If true will force unlock the index on startup
        /// </summary>
        public bool UnlockIndex { get; set; }
    }
}
