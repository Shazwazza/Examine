using Lucene.Net.Store;

namespace Examine.RemoveDirectory
{
    public class RemoteIndexOutputFactory : IRemoteIndexOutputFactory
    {
        public IndexOutput CreateIndexOutput(RemoteSyncDirectory azureSyncDirectory,  string name)
        {
            return new RemoteDirectoryIndexOutput(azureSyncDirectory, name);
        }
    }
}
