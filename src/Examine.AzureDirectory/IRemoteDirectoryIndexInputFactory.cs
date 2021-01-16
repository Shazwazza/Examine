using Azure.Storage.Blobs;
using Examine.RemoteDirectory;
using Lucene.Net.Store;

namespace Examine.AzureDirectory
{
    public interface IRemoteDirectoryIndexInputFactory
    {
        IndexInput GetIndexInput(AzureLuceneDirectory azuredirectory, IRemoteDirectory helper, string name);
    }
}