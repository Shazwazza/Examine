using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Examine.LuceneEngine.Directories;
using Examine.LuceneEngine.MergeShedulers;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Examine.AzureDirectory
{
    /// <summary>
    /// A Lucene directory used to store master index files in blob storage and sync local files to a %temp% fast drive storage
    /// </summary>
    public class AzureDirectory : ExamineDirectory
    {
        private readonly bool _isReadOnly;
        private volatile bool _dirty = true;
        private bool _inSync = false;
        private readonly object _locker = new object();
                
        private readonly string _containerName;        
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;
        private readonly LockFactory _lockFactory;

        /// <summary>
        /// Create an AzureDirectory
        /// </summary>
        /// <param name="storageAccount">storage account to use</param>
        /// <param name="containerName">name of container (folder in blob storage)</param>
        /// <param name="cacheDirectory">local Directory object to use for local cache</param>
        /// <param name="compressBlobs"></param>
        /// <param name="rootFolder">path of the root folder inside the container</param>
        /// <param name="isReadOnly">
        /// By default this is set to false which means that the <see cref="LockFactory"/> created for this directory will be 
        /// a <see cref="MultiIndexLockFactory"/> which will create locks in both the cache and blob storage folders.
        /// If this is set to true, the lock factory will be the default LockFactory configured for the cache directorty.
        /// </param>
        public AzureDirectory(
            CloudStorageAccount storageAccount,            
            string containerName,
            Lucene.Net.Store.Directory cacheDirectory,
            bool compressBlobs = false,
            string rootFolder = null,
            bool isReadOnly = false)
        {            
            if (storageAccount == null) throw new ArgumentNullException(nameof(storageAccount));
            if (cacheDirectory == null) throw new ArgumentNullException(nameof(cacheDirectory));
            if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(containerName));
            _isReadOnly = isReadOnly;
            IsReadOnly = _isReadOnly;
            CacheDirectory = cacheDirectory;
            _containerName = containerName.ToLower();
            MergeScheduler = new NoMergeSheduler();
            _lockFactory = isReadOnly
                ? CacheDirectory.LockFactory
                : new MultiIndexLockFactory(new AzureDirectorySimpleLockFactory(this), CacheDirectory.LockFactory);

            if (string.IsNullOrEmpty(rootFolder))
                RootFolder = string.Empty;
            else
            {
                rootFolder = rootFolder.Trim('/');
                RootFolder = rootFolder + "/";
            }

            _blobClient = storageAccount.CreateCloudBlobClient();
            EnsureContainer();
            CompressBlobs = compressBlobs;
        }

        public string RootFolder { get; }
        public CloudBlobContainer BlobContainer => _blobContainer;
        public bool CompressBlobs { get; }
        public Lucene.Net.Store.Directory CacheDirectory { get; }

        public void ClearCache()
        {
            foreach (string file in CacheDirectory.ListAll())
            {
                CacheDirectory.DeleteFile(file);
            }
        }

        public void EnsureContainer()
        {
            _blobContainer = _blobClient.GetContainerReference(_containerName);
            _blobContainer.CreateIfNotExists();
        }
        
        public override string[] ListAll()
        {
            var blobFiles = CheckDirty();

            return _inSync 
                ? CacheDirectory.ListAll() 
                : (blobFiles ?? GetAllBlobFiles());
        }

        private string[] GetAllBlobFiles()
        {
            var results = from blob in _blobContainer.ListBlobs(RootFolder)
                          select blob.Uri.AbsolutePath.Substring(blob.Uri.AbsolutePath.LastIndexOf('/') + 1);
            return results.ToArray();
        }

        /// <summary>Returns true if a file with the given name exists. </summary>
        public override bool FileExists(String name)
        {
            CheckDirty();

            if (_inSync)
            {
                try
                {
                    return CacheDirectory.FileExists(name);
                }
                catch (Exception)
                {
                    //revert to checking the master - what implications would this have?
                    try
                    {
                        return _blobContainer.GetBlockBlobReference(RootFolder + name).Exists();
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            try
            {
                return _blobContainer.GetBlockBlobReference(RootFolder + name).Exists();
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Returns the time the named file was last modified. </summary>
        public override long FileModified(String name)
        {
            CheckDirty();

            if (_inSync)
            {
                return CacheDirectory.FileModified(name);
            }

            try
            {
                var blob = _blobContainer.GetBlockBlobReference(RootFolder + name);
                blob.FetchAttributes();
                if (blob.Properties.LastModified != null)
                {
                    var utcDate = blob.Properties.LastModified.Value.UtcDateTime;

                    //This is the data structure of how the default Lucene FSDirectory returns this value so we want
                    // to be consistent with how Lucene works
                    return (long)utcDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>Set the modified time of an existing file to now. </summary>
        [Obsolete("This is actually never used")]
        public override void TouchFile(string name)
        {
            //just update the cache file - the Lucene source actually never calls this method!
            CacheDirectory.TouchFile(name);
            SetDirty();
        }

        /// <summary>Removes an existing file in the directory. </summary>
        public override void DeleteFile(string name)
        {
            //We're going to try to remove this from the cache directory first,
            // because the IndexFileDeleter will call this file to remove files 
            // but since some files will be in use still, it will retry when a reader/searcher
            // is refreshed until the file is no longer locked. So we need to try to remove 
            // from local storage first and if it fails, let it keep throwing the IOExpception
            // since that is what Lucene is expecting in order for it to retry.
            //If we remove the main storage file first, then this will never retry to clean out
            // local storage because the FileExist method will always return false.
            try
            {
                if (CacheDirectory.FileExists(name + ".blob"))
                {
                    CacheDirectory.DeleteFile(name + ".blob");
                }

                if (CacheDirectory.FileExists(name))
                {
                    CacheDirectory.DeleteFile(name);
                    SetDirty();
                }
            }
            catch (IOException ex)
            {
                //This will occur because this file is locked, when this is the case, we don't really want to delete it from the master either because
                // if we do that then this file will never get removed from the cache folder either! This is based on the Deletion Policy which the
                // IndexFileDeleter uses. We could implement our own one of those to deal with this scenario too but it seems the easiest way it to just 
                // let this throw so Lucene will retry when it can and when that is successful we'll also clear it from the master
                throw;
            }

            //if we are readonly, then we are only modifying local storage
            if (_isReadOnly) return;

            //if we've made it this far then the cache directly file has been successfully removed so now we'll do the master
            
            var blob = _blobContainer.GetBlockBlobReference(RootFolder + name);
            blob.DeleteIfExists();            
            SetDirty();

            Trace.WriteLine($"DELETE {_blobContainer.Uri}/{name}");
        }

        
        /// <summary>Returns the length of a file in the directory. </summary>
        public override long FileLength(string name)
        {
            CheckDirty();

            if (_inSync)
            {
                return CacheDirectory.FileLength(name);
            }

            var blob = _blobContainer.GetBlockBlobReference(RootFolder + name);
            blob.FetchAttributes();

            // index files may be compressed so the actual length is stored in metatdata
            var hasMetadataValue = blob.Metadata.TryGetValue("CachedLength", out var blobLegthMetadata);

            if (hasMetadataValue && long.TryParse(blobLegthMetadata, out var blobLength))
            {
                return blobLength;
            }
            return blob.Properties.Length; // fall back to actual blob size
        }

        /// <summary>Creates a new, empty file in the directory with the given name.
        /// Returns a stream writing this file. 
        /// </summary>
        public override IndexOutput CreateOutput(string name)
        {
            SetDirty();

            //if we are readonly, then we are only modifying local storage
            if (_isReadOnly)
            {
                return CacheDirectory.CreateOutput(name);
            }

            var blob = _blobContainer.GetBlockBlobReference(RootFolder + name);
            return new AzureIndexOutput(this, blob, name);
        }

        /// <summary>Returns a stream reading an existing file. </summary>
        public override IndexInput OpenInput(string name)
        {
            CheckDirty();
            
            if (_inSync)
            {
                try
                {
                    return CacheDirectory.OpenInput(name);
                }
                catch (FileNotFoundException)
                {
                    //if it's not found then we need to re-read from blob so were not in sync
                    SetDirty();
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Could not get local file though we are marked as inSync, reverting to try blob storage; " + ex);
                }
            }

            //try to sync the file from blob storage

            try
            {
                var blob = _blobContainer.GetBlockBlobReference(RootFolder + name);
                blob.FetchAttributes();
                return new AzureIndexInput(this, blob);
            }
            catch (StorageException err)
            {
                throw new FileNotFoundException(name, err);
            }
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
            _lockFactory.ClearLock(name);            
        }

        public override LockFactory LockFactory => _lockFactory;

        protected override void Dispose(bool disposing)
        {
            _blobContainer = null;
            _blobClient = null;
        }

        /// <summary> Return a string identifier that uniquely differentiates
        /// this Directory instance from other Directory instances.
        /// This ID should be the same if two Directory instances
        /// (even in different JVMs and/or on different machines)
        /// are considered "the same index".  This is how locking
        /// "scopes" to the right index.
        /// </summary>
        public override string GetLockId()
        {
            return string.Concat(base.GetLockId(), CacheDirectory.GetLockId());
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
            };
        }
        
        /// <summary>
        /// Checks dirty flag and sets the _inSync flag after querying the blob strorage vs local storage segment gen
        /// </summary>
        /// <returns>
        /// If _dirty is true and blob storage files are looked up, this will return those blob storage files, this is a performance gain so
        /// we don't double query blob storage.
        /// </returns>
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
                        var blobFiles = GetAllBlobFiles();
                        var masterSeg = SegmentInfos.GetCurrentSegmentGeneration(blobFiles);
                        var localSeg = SegmentInfos.GetCurrentSegmentGeneration(CacheDirectory);
                        _inSync = masterSeg == localSeg && masterSeg != -1;
                        _dirty = false;
                        return blobFiles;
                    }
                }
            }

            return null;
        }
        
        private void SetDirty()
        {
            if (!_dirty)
            {
                lock (_locker)
                {
                    _dirty = true;
                }
            }
        }
    }

}
