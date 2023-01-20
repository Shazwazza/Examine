using System;
using Lucene.Net.Store;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Creates a Lucene <see cref="Lucene.Net.Store.Directory"/> for a suggester
    /// </summary>
    /// <remarks>
    /// The directory created must only be created ONCE per suggester and disposed when the factory is disposed.
    /// </remarks>
    public interface ISuggesterDirectoryFactory : IDisposable
    {
        /// <summary>
        /// Creates the directory instance
        /// </summary>
        /// <param name="forceUnlock">If true, will force unlock the directory when created</param>
        /// <returns></returns>
        /// <remarks>
        /// Any subsequent calls for the same index will return the same directory instance
        /// </remarks>
        Directory CreateDirectory(string name, bool forceUnlock);
    }
}
