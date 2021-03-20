using Azure.Storage.Blobs;
using Lucene.Net.Store;

namespace Examine.AzureDirectory
{
    public interface IAzureIndexOutputFactory
    {
        IndexOutput CreateIndexOutput(AzureDirectory azureDirectory, BlobClient blob, string name);
    }
}