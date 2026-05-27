using System;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    public class GenericDirectoryFactory : DirectoryFactoryBase
    {
        private readonly Func<string, Directory> _factory;

        public GenericDirectoryFactory(Func<string, Directory> factory)
        {
            _factory = factory;
        }

        internal GenericDirectoryFactory(Func<string, Directory> factory, bool externallyManaged)
        {
            _factory = factory;
            ExternallyManaged = externallyManaged;
        }

        /// <summary>
        /// When set to true, indicates that the directory is managed externally and will be disposed of by the caller, not the index.
        /// </summary>
        internal bool ExternallyManaged { get; }

        protected override Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
        {
            var dir = _factory(luceneIndex.Name);
            if (forceUnlock)
            {
                IndexWriter.Unlock(dir);
            }
            return dir;
        }
    }
}
