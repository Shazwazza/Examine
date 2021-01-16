using Azure.Storage.Blobs;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.RemoteDirectory;

namespace Examine.AzureDirectory
{
    public class RemoteDirectoryIndexInputFactory : IRemoteDirectoryIndexInputFactory
    {
        public IndexInput GetIndexInput(AzureLuceneDirectory azuredirectory, IRemoteDirectory helper, string name)
        {
            return new RemoteDirectoryIndexInput(azuredirectory, helper, name);
        }
    }
}
