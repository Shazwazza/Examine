using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Azure.Storage.Blobs;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.AzureDirectory
{
    public class AzureReadOnlyLuceneDirectory : AzureLuceneDirectory
    {
        private readonly string _cacheDirectoryPath;
        private readonly string _cacheDirectoryName;
        private string OldIndexFolderName;

        public AzureReadOnlyLuceneDirectory(
            string storageAccount,
            string containerName,
            string cacheDirectoryPath,
            string cacheDirectoryName,
            bool compressBlobs = false,
            string rootFolder = null) : base(storageAccount, containerName, null, compressBlobs, rootFolder)
        {
            _cacheDirectoryPath = cacheDirectoryPath;
            _cacheDirectoryName = cacheDirectoryName;
            IsReadOnly = true;
            if (CacheDirectory == null)
            {
                Trace.WriteLine("INFO CacheDirectory null. Creating or rebuilding cache");
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

        public void ResyncCache()
        {
           
            foreach (string file in GetAllBlobFiles())
            {
                if (CacheDirectory.FileExists(file))
                {
                    CacheDirectory.TouchFile(file);
                }
                var blob = GetBlobClient(RootFolder + file);
                _helper.SyncFile(CacheDirectory,blob, file,RootFolder,CompressBlobs);
            }
        }
        protected override void HandleOutOfSync()
        {
            ResyncCache();
        }
        private object _rebuildLock = new object();
        public override void RebuildCache()
        {
            lock (_rebuildLock)
            {

                //Needs locking
                Trace.WriteLine("INFO Rebuilding cache");
                var tempDir = new DirectoryInfo(
                    Path.Combine(_cacheDirectoryPath,
                        _cacheDirectoryName, DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssfffffff")));
                if (tempDir.Exists == false)
                    tempDir.Create();
                Lucene.Net.Store.Directory newIndex = new SimpleFSDirectory(tempDir);
                var lockprefix = LockFactory.LockPrefix;
                foreach (string file in GetAllBlobFiles())
                {
                    //   newIndex.TouchFile(file);
                    if (file.EndsWith(".lock"))
                    {
                        continue;
                    }
                    var blob = _blobContainer.GetBlobClient(RootFolder + file);
                    _helper.SyncFile(newIndex, blob, file,RootFolder,CompressBlobs);
                }

                var oldIndex = CacheDirectory;
                newIndex.Dispose();
                newIndex = new SimpleFSDirectory(tempDir);

                CacheDirectory = newIndex;
                _lockFactory = newIndex.LockFactory;
                if (oldIndex != null)
                {
                    try
                    {
                        oldIndex.ClearLock(lockprefix+"-write.lock");
                    }
                    catch (Exception ex)
                    {

                    }
                    foreach (var file in oldIndex.ListAll())
                    {
                        if (oldIndex.FileExists(file))
                        {
                            oldIndex.DeleteFile(file);
                        }
                    }
                    oldIndex.Dispose();
                    DirectoryInfo oldindex = new DirectoryInfo(Path.Combine(_cacheDirectoryPath,
                        _cacheDirectoryName, OldIndexFolderName));
                    oldindex.Delete();
                }
                OldIndexFolderName = tempDir.Name;
            }
        }
    }
}