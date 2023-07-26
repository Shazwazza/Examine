using System;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    public class GenericDirectoryFactory : DirectoryFactoryBase
    {
        private readonly Func<string, Directory> _factory;
        private readonly Func<string, Directory> _taxonomyDirectoryFactory;
        
        /// <inheritdoc/>
        public GenericDirectoryFactory(Func<string, Directory> factory, Func<string, Directory> taxonomyDirectoryFactory = null)
        {
            _factory = factory;
            _taxonomyDirectoryFactory = taxonomyDirectoryFactory;
        }

        /// <inheritdoc/>
        protected override Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
        {
            Directory dir = _factory(luceneIndex.Name);
            if (forceUnlock)
            {
                IndexWriter.Unlock(dir);
            }
            return dir;
        }

        /// <inheritdoc/>
        protected override Directory CreateTaxonomyDirectory(LuceneIndex luceneIndex, bool forceUnlock)
        {
            Directory dir = _taxonomyDirectoryFactory(luceneIndex.Name + "taxonomy");
            if (forceUnlock)
            {
                IndexWriter.Unlock(dir);
            }
            return dir;
        }
    }
}
