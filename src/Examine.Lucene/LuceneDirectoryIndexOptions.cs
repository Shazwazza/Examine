
using Lucene.Net.Store;

namespace Examine.Lucene
{
    public class LuceneDirectoryIndexOptions : LuceneIndexOptions
    {
        public Directory IndexDirectory { get; set; }
    }
}
