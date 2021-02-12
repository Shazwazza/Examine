using Examine.Logging;
using Lucene.Net.Store;

namespace Examine.RemoteDirectory
{
    public interface IRemoteIndexOutputFactory
    {
        IndexOutput CreateIndexOutput(RemoteSyncDirectory azureSyncDirectory, string name, ILoggingService loggingService);
    }
}