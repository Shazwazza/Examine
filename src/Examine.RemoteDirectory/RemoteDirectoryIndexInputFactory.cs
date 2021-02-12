using Examine.Logging;
using Lucene.Net.Store;

namespace Examine.RemoteDirectory
{
    public class RemoteDirectoryIndexInputFactory : IRemoteDirectoryIndexInputFactory
    {
        public IndexInput GetIndexInput(RemoteSyncDirectory azuredirectory, IRemoteDirectory helper, string name, ILoggingService loggingService)
        {
            return new RemoteDirectoryIndexInput(azuredirectory, helper, name, loggingService);
        }
    }
}
