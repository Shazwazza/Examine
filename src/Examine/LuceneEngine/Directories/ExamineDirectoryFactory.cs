using System.IO;
using Lucene.Net.Index;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.LuceneEngine.Directories
{
    public abstract class ExamineDirectoryFactory : DirectoryFactory, IDirectoryFactory2
    {
        public abstract override Directory CreateDirectory(DirectoryInfo luceneIndexFolder);
           
        public MergePolicy GetMergePolicy(IndexWriter writer)
        {
            return null;
        }

        public bool IsReadOnly { get; } = false;
   

    }
}