using System;
using System.Diagnostics;
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
        private readonly AzureLuceneDirectory _azureDirectory;

        public AzureSimpleLock(string lockFile, AzureLuceneDirectory directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (string.IsNullOrWhiteSpace(lockFile)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(lockFile));
            _lockFile = lockFile;
            _azureDirectory = directory;
        }

        public override bool IsLocked()
        {
            try
            {
                var blob = _azureDirectory.BlobContainer.GetBlobClient(_lockFile);
                var response = blob.Exists();
                return response.Value;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ERROR {ex.ToString()} Error while checking if index locked");
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
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write(_lockFile);
                            blob.Upload(stream);
                        }
                    }
                }
                catch (Azure.RequestFailedException ex) when (ex.Status == 409)//already exists unable to overwrite
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ERROR {ex.ToString()} Error while obtaining lock");
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
                Trace.WriteLine($"ERROR {ex.ToString()} Error while releasing lock");
                throw;
            }
        }
    }


}
