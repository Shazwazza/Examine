using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Lucene.Net.Store;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Examine.AzureDirectory
{
    /// <summary>
    /// Implements lock semantics on AzureDirectory native blog storage lease locks
    /// </summary>
    public class AzureNativeLock : Lock
    {
        private readonly string _lockFile;
        private readonly AzureDirectory _azureDirectory;
        private string _leaseid;

        public AzureNativeLock(string lockFile, AzureDirectory directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (string.IsNullOrWhiteSpace(lockFile)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(lockFile));
            _lockFile = lockFile;
            _azureDirectory = directory;
        }

        private bool HandleWebException(ICloudBlob blob, StorageException err)
        {
            if (err.RequestInformation.HttpStatusCode == 404 || err.RequestInformation.HttpStatusCode == 409)
            {
                _azureDirectory.EnsureContainer();
                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(_lockFile);
                    blob.UploadFromStream(stream);
                }
                return true;
            }
            return false;
        }

        public override bool IsLocked()
        {
            var blob = _azureDirectory.BlobContainer.GetBlockBlobReference(_lockFile);
            
            try
            {
                //is there a current lease id in mem?
                if (string.IsNullOrEmpty(_leaseid))
                {
                    //pass in null - propose no lease id, 
                    // https://msdn.microsoft.com/en-us/library/microsoft.windowsazure.storage.blob.cloudblobcontainer.acquirelease.aspx
                    // https://www.simple-talk.com/cloud/platform-as-a-service/azure-blob-storage-part-8-blob-leases/
                    //if this already has a lease on this file, this will result in a failure (409 – conflict); which will be caught 
                    //in the exception handler
                    var tempLease = blob.AcquireLease(TimeSpan.FromSeconds(60), null);

                    //TODO: Will this ever happen? A null result is not in the docs
                    if (string.IsNullOrEmpty(tempLease))
                    {
                        return true;
                    }

                    blob.ReleaseLease(new AccessCondition() { LeaseId = tempLease });
                }

                //TODO: It IS locked when there is no lease id?
                return string.IsNullOrEmpty(_leaseid);
            }
            catch (StorageException webErr)
            {
                if (webErr.RequestInformation.HttpStatusCode == 409)
                {
                    //there is alraedy a lease on this blob! 
                    //TODO: Can we store the ID?
                    return true;
                }

                if (HandleWebException(blob, webErr))
                    return IsLocked();
            }
            _leaseid = null;
            return false;
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

    /// <summary>
    /// Implements lock semantics on AzureDirectory via a blob lease
    /// </summary>
    public class AzureLockOriginal : Lock
    {
        private string _lockFile;
        private AzureDirectory _azureDirectory;
        private string _leaseid;

        public AzureLockOriginal(string lockFile, AzureDirectory directory)
        {
            _lockFile = lockFile;
            _azureDirectory = directory;
        }

        #region Lock methods
        public override bool IsLocked()
        {
            var blob = _azureDirectory.BlobContainer.GetBlockBlobReference(_lockFile);
            try
            {
                Debug.Print("IsLocked() : {0}", _leaseid);
                if (String.IsNullOrEmpty(_leaseid))
                {
                    var tempLease = blob.AcquireLease(TimeSpan.FromSeconds(60), _leaseid);
                    if (String.IsNullOrEmpty(tempLease))
                    {
                        Debug.Print("IsLocked() : TRUE");
                        return true;
                    }
                    blob.ReleaseLease(new AccessCondition() { LeaseId = tempLease });
                }
                Debug.Print("IsLocked() : {0}", _leaseid);
                return String.IsNullOrEmpty(_leaseid);
            }
            catch (StorageException webErr)
            {
                if (_handleWebException(blob, webErr))
                    return IsLocked();
            }
            _leaseid = null;
            return false;
        }

        public override bool Obtain()
        {
            var blob = _azureDirectory.BlobContainer.GetBlockBlobReference(_lockFile);
            try
            {
                Debug.Print("AzureLock:Obtain({0}) : {1}", _lockFile, _leaseid);
                if (String.IsNullOrEmpty(_leaseid))
                {
                    _leaseid = blob.AcquireLease(TimeSpan.FromSeconds(60), _leaseid);
                    Debug.Print("AzureLock:Obtain({0}): AcquireLease : {1}", _lockFile, _leaseid);

                    // keep the lease alive by renewing every 30 seconds
                    long interval = (long)TimeSpan.FromSeconds(30).TotalMilliseconds;
                    _renewTimer = new Timer((obj) =>
                    {
                        try
                        {
                            AzureLockOriginal al = (AzureLockOriginal)obj;
                            al.Renew();
                        }
                        catch (Exception err) { Debug.Print(err.ToString()); }
                    }, this, interval, interval);
                }
                return !String.IsNullOrEmpty(_leaseid);
            }
            catch (StorageException webErr)
            {
                if (_handleWebException(blob, webErr))
                    return Obtain();
            }
            return false;
        }

        private Timer _renewTimer;

        public void Renew()
        {
            if (!String.IsNullOrEmpty(_leaseid))
            {
                Debug.Print("AzureLock:Renew({0} : {1}", _lockFile, _leaseid);
                var blob = _azureDirectory.BlobContainer.GetBlockBlobReference(_lockFile);
                blob.RenewLease(new AccessCondition { LeaseId = _leaseid });
            }
        }

        public override void Release()
        {
            Debug.Print("AzureLock:Release({0}) {1}", _lockFile, _leaseid);
            if (!String.IsNullOrEmpty(_leaseid))
            {
                var blob = _azureDirectory.BlobContainer.GetBlockBlobReference(_lockFile);
                blob.ReleaseLease(new AccessCondition { LeaseId = _leaseid });
                if (_renewTimer != null)
                {
                    _renewTimer.Dispose();
                    _renewTimer = null;
                }
                _leaseid = null;
            }
        }
        #endregion

        public void BreakLock()
        {
            Debug.Print("AzureLock:BreakLock({0}) {1}", _lockFile, _leaseid);
            var blob = _azureDirectory.BlobContainer.GetBlockBlobReference(_lockFile);
            try
            {
                blob.BreakLease();
            }
            catch (Exception)
            {
            }
            _leaseid = null;
        }

        public override System.String ToString()
        {
            return String.Format("AzureLock@{0}.{1}", _lockFile, _leaseid);
        }

        private bool _handleWebException(ICloudBlob blob, StorageException err)
        {
            if (err.RequestInformation.HttpStatusCode == 404 || err.RequestInformation.HttpStatusCode == 409)
            {
                _azureDirectory.EnsureContainer();
                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(_lockFile);
                    blob.UploadFromStream(stream);
                }
                return true;
            }
            return false;
        }

    }


    //TODO: Mimic this source:
    //internal class NativeFSLock : Lock
    //{
    //    private FileStream Channel;
    //    private readonly DirectoryInfo Path;
    //    private readonly NativeFSLockFactory _creatingInstance;
    //    private readonly DirectoryInfo LockDir;

    //    public NativeFSLock(NativeFSLockFactory creatingInstance, DirectoryInfo lockDir, string lockFileName)
    //    {
    //        _creatingInstance = creatingInstance;
    //        this.LockDir = lockDir;
    //        Path = new DirectoryInfo(System.IO.Path.Combine(lockDir.FullName, lockFileName));
    //    }

    //    public override bool Obtain()
    //    {
    //        lock (this)
    //        {
    //            FailureReason = null;

    //            if (Channel != null)
    //            {
    //                // Our instance is already locked:
    //                return false;
    //            }

    //            if (!System.IO.Directory.Exists(LockDir.FullName))
    //            {
    //                try
    //                {
    //                    System.IO.Directory.CreateDirectory(LockDir.FullName);
    //                }
    //                catch
    //                {
    //                    throw new System.IO.IOException("Cannot create directory: " + LockDir.FullName);
    //                }
    //            }
    //            else if (File.Exists(LockDir.FullName))
    //            {
    //                throw new IOException("Found regular file where directory expected: " + LockDir.FullName);
    //            }

    //            var success = false;
    //            try
    //            {
    //                Channel = new FileStream(Path.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
    //                Channel.Lock(0, Channel.Length);
    //                success = true;
    //            }
    //            catch (IOException e)
    //            {
    //                FailureReason = e;
    //                IOUtils.CloseWhileHandlingException(Channel);
    //                Channel = null;
    //            }
    //            // LUCENENET: UnauthorizedAccessException does not derive from IOException like in java
    //            catch (UnauthorizedAccessException e)
    //            {
    //                // On Windows, we can get intermittent "Access
    //                // Denied" here.  So, we treat this as failure to
    //                // acquire the lock, but, store the reason in case
    //                // there is in fact a real error case.
    //                FailureReason = e;
    //                IOUtils.CloseWhileHandlingException(Channel);
    //                Channel = null;
    //            }
    //            finally
    //            {
    //                if (!success)
    //                {
    //                    IOUtils.CloseWhileHandlingException(Channel);
    //                    Channel = null;
    //                }
    //            }

    //            return Channel != null;
    //        }
    //    }

    //    public override void Dispose()
    //    {
    //        // LUCENENET: No lock to release, just dispose the channel
    //        if (Channel != null)
    //        {
    //            Channel.Dispose();
    //            Channel = null;
    //        }
    //    }

    //    public override void Release()
    //    {
    //        lock (this)
    //        {
    //            if (Channel != null)
    //            {
    //                try
    //                {
    //                    Channel.Unlock(0, Channel.Length);

    //                    NativeFSLock _;
    //                    _creatingInstance._locks.TryRemove(Path.FullName, out _);
    //                }
    //                finally
    //                {
    //                    IOUtils.CloseWhileHandlingException(Channel);
    //                    Channel = null;
    //                }

    //                bool tmpBool;
    //                if (File.Exists(Path.FullName))
    //                {
    //                    File.Delete(Path.FullName);
    //                    tmpBool = true;
    //                }
    //                else if (System.IO.Directory.Exists(Path.FullName))
    //                {
    //                    System.IO.Directory.Delete(Path.FullName);
    //                    tmpBool = true;
    //                }
    //                else
    //                {
    //                    tmpBool = false;
    //                }
    //                if (!tmpBool)
    //                {
    //                    throw new LockReleaseFailedException("failed to delete " + Path);
    //                }
    //            }
    //        }
    //    }

    //    public override bool IsLocked
    //    {
    //        get
    //        {
    //            lock (this)
    //            {
    //                // The test for is isLocked is not directly possible with native file locks:

    //                // First a shortcut, if a lock reference in this instance is available
    //                if (Channel != null)
    //                {
    //                    return true;
    //                }

    //                // Look if lock file is present; if not, there can definitely be no lock!
    //                bool tmpBool;
    //                if (System.IO.File.Exists(Path.FullName))
    //                    tmpBool = true;
    //                else
    //                    tmpBool = System.IO.Directory.Exists(Path.FullName);
    //                if (!tmpBool)
    //                    return false;

    //                // Try to obtain and release (if was locked) the lock
    //                try
    //                {
    //                    bool obtained = Obtain();
    //                    if (obtained)
    //                    {
    //                        Release();
    //                    }
    //                    return !obtained;
    //                }
    //                catch (IOException)
    //                {
    //                    return false;
    //                }
    //            }
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        return "NativeFSLock@" + Path;
    //    }
    //}
}