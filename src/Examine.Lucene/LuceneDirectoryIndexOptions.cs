
using Examine.Lucene.Directories;
using Lucene.Net.Store;

namespace Examine.Lucene
{
    public class LuceneDirectoryIndexOptions : LuceneIndexOptions
    {
        public IDirectoryFactory DirectoryFactory { get; set; }
    }
}
