using Azure.Storage.Blobs;
using Lucene.Net.Store;

namespace Examine.AzureDirectory
{
    public class AzureIndexOutputFactory : IAzureIndexOutputFactory
    {
        public IndexOutput CreateIndexOutput(AzureDirectory azureDirectory, BlobClient blob, string name)
        {
            return new AzureIndexOutput(azureDirectory, blob, name);
        }
    }
}