using System;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    /// <summary>
    /// Represents a generic directory factory
    /// </summary>
    public class GenericDirectoryFactory : IDirectoryFactory
    {
        private readonly Func<string, Directory> _factory;
        private readonly Func<string, Directory>? _taxonomyDirectoryFactory;

        /// <summary>
        /// Creates an instance of <see cref="GenericDirectoryFactory"/>
        /// </summary>
        public GenericDirectoryFactory(
            Func<string, Directory> factory,
            Func<string, Directory> taxonomyDirectoryFactory)
            : this(false, factory, taxonomyDirectoryFactory)
        {
        }

        private GenericDirectoryFactory(
            bool externallyManaged,
            Func<string, Directory> factory,
            Func<string, Directory>? taxonomyDirectoryFactory = null)
        {
            ExternallyManaged = externallyManaged;
            _factory = factory;
            _taxonomyDirectoryFactory = taxonomyDirectoryFactory;
        }

        /// <summary>
        /// Creates a <see cref="GenericDirectoryFactory"/> instance with externally managed directories.
        /// </summary>
        internal static GenericDirectoryFactory FromExternallyManaged(
            Func<string, Directory> factory,
            Func<string, Directory>? taxonomyDirectoryFactory = null) =>
            new(true, factory, taxonomyDirectoryFactory);

        /// <summary>
        /// When set to true, indicates that the directory is managed externally and will be disposed of by the caller, not the index.
        /// </summary>
        internal bool ExternallyManaged { get; }

        /// <inheritdoc/>
        public Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
        {
            var dir = _factory(luceneIndex.Name);
            if (forceUnlock)
            {
                IndexWriter.Unlock(dir);
            }
            return dir;
        }

        /// <inheritdoc/>
        public Directory CreateTaxonomyDirectory(LuceneIndex luceneIndex, bool forceUnlock)
        {
            if (_taxonomyDirectoryFactory is null)
            {
                throw new InvalidOperationException("Taxonomy Directory factory is null.");
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
