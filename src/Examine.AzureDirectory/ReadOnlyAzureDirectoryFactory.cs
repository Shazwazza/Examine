using Examine.Lucene.Directories;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.AzureDirectory
{
    /// <summary>
    /// The <see cref="IDirectoryFactory"/> for storing master index data in Blob storage for user on the server that only reads from the index
    /// </summary>
    public class ReadOnlyAzureDirectoryFactory : AzureDirectoryFactory
    {
        public ReadOnlyAzureDirectoryFactory(IApplicationIdentifier applicationIdentifier, ILoggerFactory loggerFactory, SyncMutexManager syncMutexManager, ILockFactory lockFactory)
            : base(applicationIdentifier, loggerFactory, syncMutexManager, lockFactory, isReadOnly: true)
        {   
        }
    }
}
