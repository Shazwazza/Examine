using System;
using System.IO;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    public class GenericDirectoryFactory : IDirectoryFactory
    {
        private readonly Func<string, Directory> _factory;

        public GenericDirectoryFactory(Func<string, Directory> factory) => _factory = factory;

        public Directory CreateDirectory(string indexName) => _factory(indexName);
    }
}
