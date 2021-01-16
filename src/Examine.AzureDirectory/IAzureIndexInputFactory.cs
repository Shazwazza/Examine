using Azure.Storage.Blobs;
using Lucene.Net.Store;

namespace Examine.AzureDirectory
{
    public interface IAzureIndexInputFactory
    {
        IndexInput GetIndexInput(AzureLuceneDirectory azuredirectory, BlobClient blob, AzureHelper helper);
    }
}