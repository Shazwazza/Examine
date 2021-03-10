using System.IO;
using System.Security;
using Examine.Logging;
using Examine.LuceneEngine.Providers;
using Directory = Lucene.Net.Store.Directory;
namespace Examine.LuceneEngine.Directories
{
    public interface IDirectoryFactory2
    {
        Directory CreateDirectory(DirectoryInfo luceneIndexFolder, ILoggingService loggingService);
    }
}