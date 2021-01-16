using Azure.Storage.Blobs;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.AzureDirectory
{
    public class AzureIndexInputFactory : IAzureIndexInputFactory
    {
        public IndexInput GetIndexInput(AzureLuceneDirectory azuredirectory, BlobClient blob, AzureHelper helper)
        {
            return new AzureIndexInput(azuredirectory, blob, helper);
        }
    }
}
