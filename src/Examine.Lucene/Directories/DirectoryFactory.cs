using System;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    /// <summary>
    /// Represents a generic directory factory
    /// </summary>
    public class GenericDirectoryFactory : DirectoryFactoryBase
    {
        private readonly Func<string, Directory> _factory;
        private readonly Func<string, Directory>? _taxonomyDirectoryFactory;

        /// <summary>
        /// Creates a an instance of <see cref="GenericDirectoryFactory"/>
        /// </summary>
        /// <param name="factory">The factory</param>
        [Obsolete("To remove in Examine V5")]
        public GenericDirectoryFactory(Func<string, Directory> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Creates a an instance of <see cref="GenericDirectoryFactory"/>
        /// </summary>
        /// <param name="factory">The factory</param>
        /// <param name="taxonomyDirectoryFactory">The taxonomy directory factory</param>
        public GenericDirectoryFactory(Func<string, Directory> factory, Func<string, Directory> taxonomyDirectoryFactory)
        {
            _factory = factory;
            _taxonomyDirectoryFactory = taxonomyDirectoryFactory;
        }

        internal GenericDirectoryFactory(Func<string, Directory> factory, Func<string, Directory> taxonomyDirectoryFactory, bool externallyManaged)
        {
            _factory = factory;
            _taxonomyDirectoryFactory = taxonomyDirectoryFactory;
            ExternallyManaged = externallyManaged;
        }
        
        /// <summary>
        /// When set to true, indicates that the directory is managed externally and will be disposed of by the caller, not the index.
        /// </summary>
        internal bool ExternallyManaged { get; }
        
        /// <inheritdoc/>
        protected override Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
        {
            var dir = _factory(luceneIndex.Name);
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

            var dir = _taxonomyDirectoryFactory(luceneIndex.Name + "taxonomy");
            if (forceUnlock)
            {
                IndexWriter.Unlock(dir);
            }
            return dir;
        }
    }
}
