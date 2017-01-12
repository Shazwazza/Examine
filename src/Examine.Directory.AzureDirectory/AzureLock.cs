using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lucene.Net.Store;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Examine.Directory.AzureDirectory
{
    /// <summary>
    /// Implements lock semantics on AzureDirectory via a lock file - just like SimpleFSLock
    /// </summary>
    public class AzureLock : Lock
    {
        private readonly string _lockFile;
        private readonly AzureDirectory _azureDirectory;

        public AzureLock(string lockFile, AzureDirectory directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (string.IsNullOrWhiteSpace(lockFile)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(lockFile));
            _lockFile = lockFile;
            _azureDirectory = directory;
        }
        
        public override bool IsLocked()
        {
            var blob = _azureDirectory.BlobContainer.GetBlockBlobReference(_lockFile);
            return blob.Exists();
        }

        public override bool Obtain()
        {
            var blob = _azureDirectory.BlobContainer.GetBlockBlobReference(_lockFile);
            var exists = blob.Exists();
            if (exists)
                return false;

            _azureDirectory.EnsureContainer();
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(_lockFile);
                blob.UploadFromStream(stream);
            }
            return true;            
        }        

        public override void Release()
        {
            var blob = _azureDirectory.BlobContainer.GetBlockBlobReference(_lockFile);
            var flag1 = blob.Exists();
            bool flag2;
            if (blob.Exists())
            {
                blob.Delete();
                flag2 = true;
            }           
            else
                flag2 = false;
            if (flag1 && !flag2)
                throw new LockReleaseFailedException("failed to delete " + _lockFile);
        }
               

    }

}
