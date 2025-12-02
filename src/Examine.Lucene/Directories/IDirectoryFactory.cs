using System;
using Examine.Lucene.Providers;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    /// <summary>
    /// Creates a Lucene <see cref="Directory"/> for an index
    /// </summary>
    /// <remarks>
    /// Used by the index to create directory instance for the index. The index is responsible for managing the lifetime of the directory instance.
    /// </remarks>
    public interface IDirectoryFactory
    {
        /// <summary>
        /// Creates the directory instance
        /// </summary>
        /// <param name="luceneIndex"></param>
        /// <param name="forceUnlock">If true, will force unlock the directory when created</param>
        /// <returns></returns>
        /// <remarks>
        /// Any subsequent calls for the same index will return the same directory instance
        /// </remarks>
        public Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock);

        /// <summary>
        /// Creates the directory instance for the Taxonomy Index
        /// </summary>
        /// <param name="luceneIndex"></param>
        /// <param name="forceUnlock">If true, will force unlock the directory when created</param>
        /// <returns></returns>
        /// <remarks>
        /// Any subsequent calls for the same index will return the same directory instance
        /// </remarks>
        public Directory CreateTaxonomyDirectory(LuceneIndex luceneIndex, bool forceUnlock);
    }
}
