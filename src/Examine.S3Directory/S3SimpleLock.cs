﻿using System;
using System.IO;
using Lucene.Net.Store;

namespace Examine.S3Directory
{
    /// <summary>
    /// Implements lock semantics on AzureDirectory via a lock file - just like SimpleFSLock
    /// </summary>
    public class S3SimpleLock : Lock
    {
        private readonly string _lockFile;
        private readonly S3Directory _s3Directory;

        public S3SimpleLock(string lockFile, S3Directory directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (string.IsNullOrWhiteSpace(lockFile)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(lockFile));
            _lockFile = lockFile;
            _s3Directory = directory;
        }
        
        public override bool IsLocked()
        {
            var blob = _s3Directory.BlobContainer.GetBlockBlobReference(_lockFile);
            return blob.Exists();
        }

        public override bool Obtain()
        {
            var blob = _s3Directory.BlobContainer.GetBlockBlobReference(_lockFile);
            var exists = blob.Exists();
            if (exists)
                return false;

            _s3Directory.EnsureContainer();
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
            var blob = _s3Directory.BlobContainer.GetBlockBlobReference(_lockFile);
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
