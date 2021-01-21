using Lucene.Net.Store;

namespace Examine.RemoveDirectory
{
    public interface IRemoteDirectoryIndexInputFactory
    {
        IndexInput GetIndexInput(RemoteSyncDirectory azuredirectory, IRemoteDirectory helper, string name);
    }
}