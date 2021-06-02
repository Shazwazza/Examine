using System;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    public class GenericDirectoryFactory : DirectoryFactoryBase
    {
        private readonly Func<string, Directory> _factory;

        public GenericDirectoryFactory(Func<string, Directory> factory) => _factory = factory;

        protected override Directory CreateDirectory(LuceneIndex index, bool forceUnlock)
        {
            Directory dir = _factory(index.Name);
            if (forceUnlock)
            {
                IndexWriter.Unlock(dir);
            }
            return dir;
        }
    }
}
