using Lucene.Net.Store;

namespace Examine.RemoteDirectory
{
    public interface IRemoteDirectoryIndexInputFactory
    {
        IndexInput GetIndexInput(RemoteSyncDirectory azuredirectory, IRemoteDirectory helper, string name);
    }
}