using Azure.Storage.Blobs;
using Lucene.Net.Store;

namespace Examine.AzureDirectory
{
    public interface IAzureIndexOutputFactory
    {
        IndexOutput CreateIndexOutput(AzureLuceneDirectory azureDirectory, BlobClient blob, string name);
    }
}