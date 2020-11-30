using System.IO;
using System.Security;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.LuceneEngine.Directories
{
    /// <summary>
    /// Creates a Lucene <see cref="Lucene.Net.Store.Directory"/> for an index
    /// </summary>
    public interface IDirectoryFactory
    {   
        Directory CreateDirectory(DirectoryInfo luceneIndexFolder);
   
    }
}