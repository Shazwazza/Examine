using Azure.Storage.Blobs;
using Lucene.Net.Store;

namespace Examine.AzureDirectory
{
    public interface IAzureIndexInputFactory
    {
        IndexInput GetIndexInput(AzureDirectory azuredirectory, BlobClient blob);
    }
}