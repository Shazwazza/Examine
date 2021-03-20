using Azure.Storage.Blobs;
using Lucene.Net.Store;

namespace Examine.AzureDirectory
{
    public class AzureIndexInputFactory : IAzureIndexInputFactory
    {
        public IndexInput GetIndexInput(AzureDirectory azuredirectory, BlobClient blob)
        {
            return new AzureIndexInput(azuredirectory, blob);
        }
    }
}