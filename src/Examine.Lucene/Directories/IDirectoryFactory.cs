using System;
using Examine.Lucene.Providers;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    /// <summary>
    /// Creates a Lucene <see cref="Directory"/> for an index
    /// </summary>
    /// <remarks>
    /// The directory created must only be created ONCE per index and disposed when the index is disposed.
    /// </remarks>
    public interface IDirectoryFactory : IDisposable
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
        Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock);

        /// <summary>
        /// Creates the directory instance for the Taxonomy Index
        /// </summary>
        /// <param name="luceneIndex"></param>
        /// <param name="forceUnlock">If true, will force unlock the directory when created</param>
        /// <returns></returns>
        /// <remarks>
        /// Any subsequent calls for the same index will return the same directory instance
        /// </remarks>
        Directory CreateTaxonomyDirectory(LuceneIndex luceneIndex, bool forceUnlock);
    }
}
