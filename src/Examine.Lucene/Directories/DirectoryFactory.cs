using System;
using System.IO;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{

    public abstract class DirectoryFactory : IDirectoryFactory
    {
        public abstract Directory CreateDirectory(DirectoryInfo luceneIndexFolder);
    }
}
