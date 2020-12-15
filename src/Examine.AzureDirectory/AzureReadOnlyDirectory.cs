using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.AzureDirectory
{
    public class AzureReadOnlyDirectory : ExamineDirectory, IAzureDirectory
    {
        private readonly string _cacheDirectoryPath;
        private readonly string _cacheDirectoryName;
        public Lucene.Net.Store.Directory CacheDirectory { get; private set; }
        private readonly bool _isReadOnly;
        private volatile bool _dirty = true;
        private bool _inSync = false;
        private readonly object _locker = new object();

        private readonly string _containerName;
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;
        private LockFactory _lockFactory;
        private string OldIndexFolderName;
        public string RootFolder { get; set; }

        public CloudBlobContainer BlobContainer => _blobContainer;
        public bool CompressBlobs { get; }

        public AzureReadOnlyDirectory(
            CloudStorageAccount storageAccount,
            string containerName,
            string cacheDirectoryPath,
            string cacheDirectoryName,
            bool compressBlobs = false,
            string rootFolder = null,
            bool isReadOnly = false)
        {
            _cacheDirectoryPath = cacheDirectoryPath;
            _cacheDirectoryName = cacheDirectoryName;
            _containerName = containerName.ToLower();
            _blobClient = storageAccount.CreateCloudBlobClient();
            _isReadOnly = isReadOnly;
            if (string.IsNullOrEmpty(rootFolder))
                RootFolder = string.Empty;
            else
            {
                rootFolder = rootFolder.Trim('/');
                RootFolder = rootFolder + "/";
            }

            _blobContainer= this.EnsureContainer(_blobClient,_containerName);
            if (CacheDirectory == null)
            {
                CreateOrReadCache();
            }
            else
            {
                CheckDirty();
            }
           
            CompressBlobs = compressBlobs;
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
            var tempDir = new DirectoryInfo(
                Path.Combine(_cacheDirectoryPath,
                    _cacheDirectoryName, DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssfffffff")));
            if (tempDir.Exists == false)
                tempDir.Create();
            Lucene.Net.Store.Directory newIndex = new SimpleFSDirectory(tempDir);
            foreach (string file in this.GetAllBlobFiles(_blobContainer, RootFolder))
            {
             //   newIndex.TouchFile(file);
                var blob = _blobContainer.GetBlockBlobReference(RootFolder + file);
                this.SyncFile(newIndex,blob, file);
            }

            var oldIndex = CacheDirectory;
            newIndex.Dispose();
            newIndex = new SimpleFSDirectory(tempDir);
            
            CacheDirectory = newIndex;
            _lockFactory = newIndex.LockFactory;
            if (oldIndex != null)
            {
                oldIndex.ClearLock("write.lock");
                foreach (var file in   oldIndex.ListAll())
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
        public virtual bool ShouldCompressFile(string path)
        {
            if (!CompressBlobs)
                return false;

            var ext = System.IO.Path.GetExtension(path);
            switch (ext)
            {
                case ".cfs":
                case ".fdt":
                case ".fdx":
                case ".frq":
                case ".tis":
                case ".tii":
                case ".nrm":
                case ".tvx":
                case ".tvd":
                case ".tvf":
                case ".prx":
                    return true;
                default:
                    return false;
            }

            ;
        }
        /// <summary>Construct a {@link Lock}.</summary>
        /// <param name="name">the name of the lock file
        /// </param>
        public override Lock MakeLock(string name)
        {
            return _lockFactory.MakeLock(name);
        }

        public override void ClearLock(string name)
        {
            if (!_isReadOnly)
            {
                _lockFactory.ClearLock(name);
            }

            CacheDirectory.ClearLock(name);
        }

        public override LockFactory LockFactory => _lockFactory;
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
        private string[] CheckDirty()
        {
            if (_dirty)
            {
                lock (_locker)
                {
                    //double check locking
                    if (_dirty)
                    {
                        //these methods don't throw exceptions, will return -1 if something has gone wrong
                        // in which case we'll consider them not in sync
                        var blobFiles = this.GetAllBlobFiles(_blobContainer, RootFolder);
                        var masterSeg = SegmentInfos.GetCurrentSegmentGeneration(blobFiles);
                        var localSeg = SegmentInfos.GetCurrentSegmentGeneration(CacheDirectory);
                        _inSync = masterSeg == localSeg && masterSeg != -1;
                        if (!_inSync)
                        {
                            RebuildCache();
                        }
                        _dirty = false;
                        
                        return blobFiles;
                    }
                }
            }

            return null;
        }
    }
}