using System;
using System.Diagnostics;
using System.IO;
using Lucene.Net.Store;

namespace Examine.RemoteDirectory
{
    /// <summary>
    /// Implements lock semantics on AzureDirectory via a lock file - just like SimpleFSLock
    /// </summary>
    public class RemoteSimpleLock : Lock
    {
        private readonly string _lockFile;
        private readonly RemoteSyncDirectory _azureSyncDirectory;
        private readonly IRemoteDirectory _remoteDirectory;

        public RemoteSimpleLock(string lockFile, RemoteSyncDirectory syncDirectory, IRemoteDirectory remoteDirectory)
        {
            if (syncDirectory == null) throw new ArgumentNullException(nameof(syncDirectory));
            if (string.IsNullOrWhiteSpace(lockFile))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(lockFile));
            _lockFile = lockFile;
            _azureSyncDirectory = syncDirectory;
            _remoteDirectory = remoteDirectory;
        }

        public override bool IsLocked()
        {
            return _remoteDirectory.FileExists(_lockFile);
        }


        public override bool Obtain()
        {
            try
            {
                if (IsLocked())
                    return false;


                using (var stream = new MemoryStream())
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(_lockFile);
                        return _remoteDirectory.Upload(stream, _lockFile);
                    }
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
                   _remoteDirectory.DeleteFile(_lockFile);
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