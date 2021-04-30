using Examine.LuceneEngine.Directories;
using Microsoft.Extensions.Logging;

namespace Examine.AzureDirectory
{
    /// <summary>
    /// The <see cref="IDirectoryFactory"/> for storing master index data in Blob storage for user on the server that only reads from the index
    /// </summary>
    public class ReadOnlyAzureDirectoryFactory : AzureDirectoryFactory
    {
        public ReadOnlyAzureDirectoryFactory(ILoggerFactory loggerFactory)
            : base(loggerFactory, isReadOnly: true)
        {   
        }
    }
}
