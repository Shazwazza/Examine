using System.IO;
using System.Security;
using Examine.Lucene.Providers;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    /// <summary>
    /// Creates a Lucene <see cref="Lucene.Net.Store.Directory"/> for an index
    /// </summary>
    public interface IDirectoryFactory
    {   
        Directory CreateDirectory(DirectoryInfo luceneIndexFolder);
    }
}