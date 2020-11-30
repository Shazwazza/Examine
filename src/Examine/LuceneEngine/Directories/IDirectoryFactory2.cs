using Lucene.Net.Index;

namespace Examine.LuceneEngine.Directories
{
    public interface IDirectoryFactory2 : IDirectoryFactory
    {
        MergePolicy GetMergePolicy(IndexWriter writer);
        bool IsReadOnly { get; }
    }
}