using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Azure.Storage.Blobs;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.AzureDirectory
{
    public class AzureReadOnlyLuceneDirectory : AzureLuceneDirectory
    {
        private readonly string _cacheDirectoryPath;
        private readonly string _cacheDirectoryName;
        private string OldIndexFolderName;

        public AzureReadOnlyLuceneDirectory(ILogger logger,
            string storageAccount,
            string containerName,
            string cacheDirectoryPath,
            string cacheDirectoryName,
            bool compressBlobs = false,
            string rootFolder = null) : base(logger,storageAccount, containerName,null,compressBlobs,rootFolder,true)
        {
            _cacheDirectoryPath = cacheDirectoryPath;
            _cacheDirectoryName = cacheDirectoryName;
            if (CacheDirectory == null)
            {
                _logger.LogInformation("CacheDirectory null. Creating or rebuilding cache");
                CreateOrReadCache();
            }
            else
            {
                CheckDirty();
            }
        }

        protected override void GuardCacheDirectory(Lucene.Net.Store.Directory cacheDirectory)
        {
            //Do nothing
        }
        private void CreateOrReadCache()
        {
            var indexParentFolder = new DirectoryInfo(
                Path.Combine(_cacheDirectoryPath,
                    _cacheDirectoryName));
            if (indexParentFolder.Exists)
            {
                var subDirectories = indexParentFolder.GetDirectories();
                if (subDirectories.Any())
                {
                    var directory = subDirectories.FirstOrDefault();
                    OldIndexFolderName = directory.Name;
                    CacheDirectory = new SimpleFSDirectory(directory);
                    _lockFactory = CacheDirectory.LockFactory;
                }
                else
                {
                    RebuildCache();
                }
            }
            else
            {
                RebuildCache();
                
            }
        }

        public void RebuildCache()
        {

            _logger.LogInformation("Rebuilding cache");
            var tempDir = new DirectoryInfo(
                Path.Combine(_cacheDirectoryPath,
                    _cacheDirectoryName, DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssfffffff")));
            if (tempDir.Exists == false)
                tempDir.Create();
            Lucene.Net.Store.Directory newIndex = new SimpleFSDirectory(tempDir);
            foreach (string file in GetAllBlobFiles())
            {
             //   newIndex.TouchFile(file);
                var blob = _blobContainer.GetBlobClient(RootFolder + file);
                SyncFile(newIndex,blob, file);
            }

            var oldIndex = CacheDirectory;
            newIndex.Dispose();
            newIndex = new SimpleFSDirectory(tempDir);
            
            CacheDirectory = newIndex;
            _lockFactory = newIndex.LockFactory;
            if (oldIndex != null)
            {
                oldIndex.ClearLock("write.lock");
                foreach (var file in oldIndex.ListAll())
                {
                    if (oldIndex.FileExists(file))
                    {
                        oldIndex.DeleteFile(file);
                    }
                }
                oldIndex.Dispose();
               DirectoryInfo oldindex  = new DirectoryInfo(Path.Combine(_cacheDirectoryPath,
                   _cacheDirectoryName,OldIndexFolderName));
               oldindex.Delete();
            }
            OldIndexFolderName = tempDir.Name;
       
        }
        private void SyncFile(Directory newIndex, BlobClient blob, string fileName)
        {
            if (this.ShouldCompressFile(fileName))
            {
                InflateStream(newIndex, blob, fileName);
            }
            else
            {
                using (var fileStream = new StreamOutput(newIndex.CreateOutput(fileName)))
                {
                    // get the blob
                    blob.DownloadTo(fileStream);
                    fileStream.Flush();
                    Trace.WriteLine($"GET {fileName} RETREIVED {fileStream.Length} bytes");

                }


            }
            // then we will get it fresh into local deflatedName 
            // StreamOutput deflatedStream = new StreamOutput(CacheDirectory.CreateOutput(deflatedName));

        }
        protected void InflateStream(Lucene.Net.Store.Directory newIndex, BlobClient blob, string fileName)
        {
            if (this.ShouldCompressFile(fileName))
            {
                InflateStream(newIndex, blob, fileName);
            }
            else
            {
                using (var fileStream = new StreamOutput(newIndex.CreateOutput(fileName)))
                {
                    // get the blob
                    blob.DownloadTo(fileStream);
                    fileStream.Flush();
                    Trace.WriteLine($"GET {fileName} RETREIVED {fileStream.Length} bytes");

                }

            }
        }
        public override string[] ListAll()
        {
            return CacheDirectory.ListAll();
        }

        public override bool FileExists(string name)
        {
            return CacheDirectory.FileExists(name);
        }

        public override long FileModified(string name)
        {
            return CacheDirectory.FileModified(name);
        }

        public override void TouchFile(string name)
        {
             CacheDirectory.TouchFile(name);
        }

        public override void DeleteFile(string name)
        {
            CacheDirectory.DeleteFile(name);
        }

        public override long FileLength(string name)
        {
            return CacheDirectory.FileLength(name);
        }

        public override IndexOutput CreateOutput(string name)
        {
            return CacheDirectory.CreateOutput(name);
        }

        public override IndexInput OpenInput(string name)
        {
            CheckDirty();
            return CacheDirectory.OpenInput(name);
        }

        protected override void Dispose(bool disposing)
        {
            CacheDirectory.Dispose();
        }
        protected virtual void HandleOutOfSync()
        {
            RebuildCache();
        }
    }
}