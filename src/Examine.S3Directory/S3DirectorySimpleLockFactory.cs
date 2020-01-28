using System.IO;
using Amazon.S3.IO;
using Lucene.Net.Store;

namespace Examine.S3Directory
{
    /// <summary>
    /// A lock factory used for S3 storage using Simple Locking (file based)
    /// </summary>
    public class S3DirectorySimpleLockFactory : LockFactory
    {
        private readonly S3Directory _s3Directory;

        public S3DirectorySimpleLockFactory(S3Directory s3Directory)
        {
            _s3Directory = s3Directory;
        }
        
        public override Lock MakeLock(string name)
        {
            if (LockPrefix != null)
                name = LockPrefix + "-" + name;

            return new S3SimpleLock(_s3Directory.RootFolder + name, _s3Directory);           
        }

        public override void ClearLock(string name)
        {
            if (LockPrefix != null)
                name = LockPrefix + "-" + name;

            var lockFile = _s3Directory.RootFolder + name;

            var blob = new S3FileInfo(_s3Directory._blobClient, _s3Directory._containerName, lockFile);
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
                throw new IOException("Cannot delete " + lockFile);            
        }
    }

   
}