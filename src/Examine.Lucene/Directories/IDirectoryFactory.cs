using System;
using Examine.Lucene.Providers;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    /// <summary>
    /// Creates a Lucene <see cref="Lucene.Net.Store.Directory"/> for an index
    /// </summary>
    /// <remarks>
    /// The directory created must only be created ONCE and disposed when the factory is disposed.
    /// </remarks>
    public interface IDirectoryFactory : IDisposable
    {   
        Directory CreateDirectory(LuceneIndex luceneIndex);
    }
}
