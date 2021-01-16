using Azure.Storage.Blobs;
using Examine.RemoteDirectory;
using Lucene.Net.Store;

namespace Examine.AzureDirectory
{
    public interface IAzureIndexOutputFactory
    {
        IndexOutput CreateIndexOutput(AzureLuceneDirectory azureDirectory, IRemoteDirectory directory, string name);
    }
}