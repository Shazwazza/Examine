using Azure.Storage.Blobs;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.AzureDirectory
{
    public class AzureIndexOutputFactory : IAzureIndexOutputFactory
    {
        public IndexOutput CreateIndexOutput(AzureLuceneDirectory azureDirectory, BlobClient blob, string name)
        {
            return new RemoteDirectoryIndexOutput(azureDirectory, blob, name);
        }
    }
}
