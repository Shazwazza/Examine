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

        protected override void Dispose(bool disposing)
        {
            throw new NotImplementedException();
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

       
               

    }


}
