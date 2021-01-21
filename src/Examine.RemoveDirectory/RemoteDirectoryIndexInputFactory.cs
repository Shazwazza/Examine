using Lucene.Net.Store;

namespace Examine.RemoveDirectory
{
    public class RemoteDirectoryIndexInputFactory : IRemoteDirectoryIndexInputFactory
    {
        public IndexInput GetIndexInput(RemoteSyncDirectory azuredirectory, IRemoteDirectory helper, string name)
        {
            return new RemoteDirectoryIndexInput(azuredirectory, helper, name);
        }
    }
}
