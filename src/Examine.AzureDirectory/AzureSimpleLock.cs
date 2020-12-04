using System;
using System.IO;
using Lucene.Net.Store;

namespace Examine.AzureDirectory
{
    /// <summary>
    /// Implements lock semantics on AzureDirectory via a lock file - just like SimpleFSLock
    /// </summary>
    public class AzureSimpleLock : Lock
    {
        private readonly string _lockFile;
        private readonly AzureDirectory _azureDirectory;

        public AzureSimpleLock(string lockFile, AzureDirectory directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (string.IsNullOrWhiteSpace(lockFile)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(lockFile));
            _lockFile = lockFile;
            _azureDirectory = directory;
        }
        
        public override bool IsLocked()
        {
            var blob = _azureDirectory.BlobContainer.GetBlobClient(_lockFile);
            var response = blob.Exists();
            return response;
        }


        public override bool Obtain()
        {
            if (IsLocked())
                return false;
            var blob = _azureDirectory.BlobContainer.GetBlobClient(_lockFile);

            _azureDirectory.EnsureContainer();
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(_lockFile);
                blob.Upload(stream);
            }
            return true;            
        }        

        public override void Release()
        {
            var flag1 = IsLocked();
            bool flag2;
            if (IsLocked())
            {
                var blob = _azureDirectory.BlobContainer.GetBlobClient(_lockFile);
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
