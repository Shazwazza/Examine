using System;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    public class GenericDirectoryFactory : DirectoryFactoryBase
    {
        private readonly Func<string, Directory> _factory;
        private readonly Func<string, Directory>? _taxonomyDirectoryFactory;

        /// <inheritdoc/>
        [Obsolete("To remove in Examine V5")]
        public GenericDirectoryFactory(Func<string, Directory> factory)
        {
            _factory = factory;
        }

        /// <inheritdoc/>
        public GenericDirectoryFactory(Func<string, Directory> factory, Func<string, Directory> taxonomyDirectoryFactory)
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
            if (_taxonomyDirectoryFactory is null)
            {
                throw new NullReferenceException("Taxonomy Directory factory is null. Use constructor with all parameters");
            }

            Directory dir = _taxonomyDirectoryFactory(luceneIndex.Name + "taxonomy");
            if (forceUnlock)
            {
                IndexWriter.Unlock(dir);
            }
            return dir;
        }
    }
}
