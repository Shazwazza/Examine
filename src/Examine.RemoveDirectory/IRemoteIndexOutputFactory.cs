using Lucene.Net.Store;

namespace Examine.RemoveDirectory
{
    public interface IRemoteIndexOutputFactory
    {
        IndexOutput CreateIndexOutput(RemoteSyncDirectory azureSyncDirectory, string name);
    }
}