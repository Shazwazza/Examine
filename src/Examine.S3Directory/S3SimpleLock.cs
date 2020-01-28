using System;
using System.IO;
using Amazon.S3.IO;
using Lucene.Net.Store;

namespace Examine.S3Directory
{
    /// <summary>
    /// Implements lock semantics on S3Directory via a lock file - just like SimpleFSLock
    /// </summary>
    public class S3SimpleLock : Lock
    {
        private readonly string _lockFile;
        private readonly S3Directory _s3Directory;

        public S3SimpleLock(string lockFile, S3Directory directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (string.IsNullOrWhiteSpace(lockFile))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(lockFile));
            _lockFile = lockFile;
            _s3Directory = directory;
        }

        public override bool IsLocked()
        {
            try
            {
                S3FileInfo s3FileInfo =
                    new S3FileInfo(_s3Directory._blobClient, _s3Directory._containerName, _lockFile);

                return s3FileInfo.Exists;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override bool Obtain()
        {
            var blob = new S3FileInfo(_s3Directory._blobClient, _s3Directory._containerName, _lockFile);
            var exists = blob.Exists;
            if (exists)
                return false;

            _s3Directory.EnsureContainer();
            using (var s3Writer = blob.OpenWrite())
            using (var writer = new StreamWriter(s3Writer))
            {
                writer.Write(_lockFile);
            }

            return true;
        }

        public override void Release()
        {
            var blob = new S3FileInfo(_s3Directory._blobClient, _s3Directory._containerName, _lockFile);
            var flag1 = blob.Exists;
            bool flag2;
            if (blob.Exists)
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