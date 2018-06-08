//using System.IO;
//using Lucene.Net.Store;

//namespace Examine.AzureDirectory
//{
//    /// <summary>
//    /// A lock factory used for azure blob storage using native blob storage leases
//    /// </summary>
//    public class AzureDirectoryNativeLockFactory : LockFactory
//    {
//        private readonly AzureDirectory _azureDirectory;        

//        public AzureDirectoryNativeLockFactory(AzureDirectory azureDirectory)
//        {
//            _azureDirectory = azureDirectory;
//        }

//        public override Lock MakeLock(string name)
//        {
//            if (lockPrefix != null)
//                name = this.lockPrefix + "-" + name;

//            return new AzureNativeLock(_azureDirectory.RootFolder + name, _azureDirectory);
//        }

//        public override void ClearLock(string name)
//        {
//            if (lockPrefix != null)
//                name = this.lockPrefix + "-" + name;

//            var lockFile = _azureDirectory.RootFolder + name;

//            var blob = _azureDirectory.BlobContainer.GetBlockBlobReference(lockFile);
//            var flag1 = blob.Exists();
//            bool flag2;
//            if (blob.Exists())
//            {
//                blob.Delete();
//                flag2 = true;
//            }
//            else
//                flag2 = false;
//            if (flag1 && !flag2)
//                throw new IOException("Cannot delete " + lockFile);
//        }
//    }
//}