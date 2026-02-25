using Examine.Lucene.Providers;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    /// <summary>
    /// Creates a Lucene <see cref="Directory"/> for taxonomy-based faceting on an index
    /// </summary>
    /// <remarks>
    /// Implement this interface alongside <see cref="IDirectoryFactory"/> to enable taxonomy-based faceting.
    /// If an <see cref="IDirectoryFactory"/> does not also implement this interface, taxonomy faceting will not be available.
    /// </remarks>
    public interface ITaxonomyDirectoryFactory
    {
        /// <summary>
        /// Creates the directory instance for the Taxonomy Index
        /// </summary>
        /// <param name="luceneIndex"></param>
        /// <param name="forceUnlock">If true, will force unlock the directory when created</param>
        /// <returns>The taxonomy directory, or null if taxonomy is not enabled for this index</returns>
        /// <remarks>
        /// Any subsequent calls for the same index will return the same directory instance.
        /// If this returns null, taxonomy-based faceting will not be available for this index.
        /// </remarks>
        public Directory? CreateTaxonomyDirectory(LuceneIndex luceneIndex, bool forceUnlock);
    }
}
