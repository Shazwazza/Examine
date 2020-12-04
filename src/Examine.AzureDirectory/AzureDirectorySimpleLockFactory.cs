using System.IO;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;

namespace Examine.AzureDirectory
{
    /// <summary>
    /// A lock factory used for azure blob storage using Simple Locking (file based)
    /// </summary>
    public class AzureDirectorySimpleLockFactory : LockFactory
    {
        private readonly AzureLuceneDirectory _azureDirectory;
        private readonly ILogger _logger;

        public AzureDirectorySimpleLockFactory(AzureLuceneDirectory azureDirectory,ILogger logger)
        {
            _azureDirectory = azureDirectory;
            _logger = logger;
        }
        
        public override Lock MakeLock(string name)
        {
            if (LockPrefix != null)
                name = LockPrefix + "-" + name;

            return new AzureSimpleLock(_azureDirectory.RootFolder + name, _azureDirectory, _logger);           
        }

        public override void ClearLock(string name)
        {
            if (LockPrefix != null)
                name = LockPrefix + "-" + name;

            var lockFile = _azureDirectory.RootFolder + name;

            var blob = _azureDirectory.BlobContainer.GetBlobClient(lockFile);
            var flag1Response = blob.Exists();
            var flag1 = flag1Response.Value;
            bool flag2;

            var flag2Response = blob.Exists();
            if (flag2Response.Value)
            {
                blob.Delete();
                flag2 = true;
            }
            else
                flag2 = false;
            if (flag1 && !flag2)
                throw new IOException("Cannot delete " + lockFile);            
        }
    }

   
}