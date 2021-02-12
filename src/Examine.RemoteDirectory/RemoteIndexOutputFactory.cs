using Examine.Logging;
using Lucene.Net.Store;

namespace Examine.RemoteDirectory
{
    public class RemoteIndexOutputFactory : IRemoteIndexOutputFactory
    {
        public IndexOutput CreateIndexOutput(RemoteSyncDirectory azureSyncDirectory,  string name, ILoggingService loggingService)
        {
            return new RemoteDirectoryIndexOutput(azureSyncDirectory, name, loggingService);
        }
    }
}
