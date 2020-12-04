using System;
using System.IO;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;

namespace Examine.AzureDirectory
{
    /// <summary>
    /// Implements lock semantics on AzureDirectory via a lock file - just like SimpleFSLock
    /// </summary>
    public class AzureSimpleLock : Lock
    {
        private readonly string _lockFile;
        private readonly AzureDirectory _azureDirectory;
        private readonly ILogger _logger;

        public AzureSimpleLock(string lockFile, AzureDirectory directory,ILogger logger)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (string.IsNullOrWhiteSpace(lockFile)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(lockFile));
            _lockFile = lockFile;
            _azureDirectory = directory;
            _logger = logger;
        }
        
        public override bool IsLocked()
        {
            try
            {
                var blob = _azureDirectory.BlobContainer.GetBlobClient(_lockFile);
                var response = blob.Exists();
                return response.Value;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error while checking if index locked");
                throw;
            }
        }


        public override bool Obtain()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while obtaining lock");
                throw;
            }
        }        

        public override void Release()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while releasing lock");
                throw;
            }
        }
    }


}
