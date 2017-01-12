using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Examine.Directory.AzureDirectory
{
    /// <summary>
    /// A Lucene directory used to store master index files in blob storage and sync local files to a %temp% fast drive storage
    /// </summary>
    public class AzureDirectory : Lucene.Net.Store.Directory
    {
        private volatile bool _dirty = true;
        private bool _inSync = false;
        private readonly object _locker = new object();
                
        private readonly string _containerName;        
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;
        private readonly Lucene.Net.Store.Directory _cacheDirectory;
        private readonly MultiIndexLockFactory _lockFactory;

        /// <summary>
        /// Create an AzureDirectory
        /// </summary>
        /// <param name="storageAccount">storage account to use</param>
        /// <param name="containerName">name of container (folder in blob storage)</param>
        /// <param name="cacheDirectory">local Directory object to use for local cache</param>
        /// <param name="compressBlobs"></param>
        /// <param name="rootFolder">path of the root folder inside the container</param>
        public AzureDirectory(
            CloudStorageAccount storageAccount,            
            string containerName,
            Lucene.Net.Store.Directory cacheDirectory,
            bool compressBlobs = false,
            string rootFolder = null)
        {            
            if (storageAccount == null) throw new ArgumentNullException(nameof(storageAccount));
            if (cacheDirectory == null) throw new ArgumentNullException(nameof(cacheDirectory));
            if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(containerName));

            _cacheDirectory = cacheDirectory;
            _containerName = containerName.ToLower();
            _lockFactory = new MultiIndexLockFactory(new AzureDirectoryLockFactory(this), _cacheDirectory.GetLockFactory());

            if (string.IsNullOrEmpty(rootFolder))
                RootFolder = string.Empty;
            else
            {
                rootFolder = rootFolder.Trim('/');
                RootFolder = rootFolder + "/";
            }

            _blobClient = storageAccount.CreateCloudBlobClient();
            EnsureContainer();
            this.CompressBlobs = compressBlobs;
        }

        public string RootFolder { get; }
        public CloudBlobContainer BlobContainer => _blobContainer;
        public bool CompressBlobs { get; set; }

        public void ClearCache()
        {
            foreach (string file in _cacheDirectory.ListAll())
            {
                _cacheDirectory.DeleteFile(file);
            }
        }

        public Lucene.Net.Store.Directory CacheDirectory => _cacheDirectory;

        public void EnsureContainer()
        {
            _blobContainer = _blobClient.GetContainerReference(_containerName);
            _blobContainer.CreateIfNotExists();
        }

        /// <summary>Returns an array of strings, one for each file in the directory. </summary>
        [Obsolete("For some Directory implementations (FSDirectory}, and its subclasses), this method silently filters its results to include only index files.  Please use ListAll instead, which does no filtering. ")]
        public override String[] List()
        {
            //proxy to the non obsolete ListAll
            return ListAll();
        }

        public override string[] ListAll()
        {
            CheckDirty();

            return _inSync ? _cacheDirectory.ListAll() : GetAllBlobFiles();
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
                    return _cacheDirectory.FileExists(name);
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
                return _cacheDirectory.FileModified(name);
            }

            try
            {
                var blob = _blobContainer.GetBlockBlobReference(RootFolder + name);
                blob.FetchAttributes();
                var utcDate = blob.Properties.LastModified.Value.UtcDateTime;

                //This is the data structure of how the default Lucene FSDirectory returns this value so we want
                // to be consistent with how Lucene works
                return (long)utcDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
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
            _cacheDirectory.TouchFile(name);
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
                if (_cacheDirectory.FileExists(name + ".blob"))
                {
                    _cacheDirectory.DeleteFile(name + ".blob");
                }

                if (_cacheDirectory.FileExists(name))
                {
                    _cacheDirectory.DeleteFile(name);
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

            //if we've made it this far then the cache directly file has been successfully removed so now we'll do the master
            
            var blob = _blobContainer.GetBlockBlobReference(RootFolder + name);
            blob.DeleteIfExists();            
            SetDirty();

            Debug.WriteLine($"DELETE {_blobContainer.Uri.ToString()}/{name}");
        }

        
        /// <summary>Renames an existing file in the directory.
        /// If a file already exists with the new name, then it is replaced.
        /// This replacement should be atomic. 
        /// </summary>
        [Obsolete("This is actually never used")]
        public override void RenameFile(string from, string to)
        {
            try
            {
                var blobFrom = _blobContainer.GetBlockBlobReference(from);
                var blobTo = _blobContainer.GetBlockBlobReference(to);
                blobTo.StartCopy(blobFrom);
                blobFrom.DeleteIfExists();
                SetDirty();               
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Could not rename file on master index; " + ex);
            }

            try
            {
                // we delete and force a redownload, since we can't do this in an atomic way
                if (_cacheDirectory.FileExists(from))
                    _cacheDirectory.RenameFile(from, to);

                // drop old cached data as it's wrong now
                if (_cacheDirectory.FileExists(from + ".blob"))
                    _cacheDirectory.DeleteFile(from + ".blob");

                SetDirty();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not rename file on local index; " + ex);
            }
        }

        /// <summary>Returns the length of a file in the directory. </summary>
        public override long FileLength(string name)
        {
            CheckDirty();

            if (_inSync)
            {
                return _cacheDirectory.FileLength(name);
            }

            var blob = _blobContainer.GetBlockBlobReference(RootFolder + name);
            blob.FetchAttributes();

            // index files may be compressed so the actual length is stored in metatdata
            string blobLegthMetadata;
            bool hasMetadataValue = blob.Metadata.TryGetValue("CachedLength", out blobLegthMetadata);

            long blobLength;
            if (hasMetadataValue && long.TryParse(blobLegthMetadata, out blobLength))
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
            var blob = _blobContainer.GetBlockBlobReference(RootFolder + name);
            return new AzureIndexOutput(this, blob, name);
        }

        /// <summary>Returns a stream reading an existing file. </summary>
        public override IndexInput OpenInput(string name)
        {
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

        public override LockFactory GetLockFactory()
        {
            return _lockFactory;
        }

        /// <summary>Closes the store. </summary>
        public override void Close()
        {
            _blobContainer = null;
            _blobClient = null;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            this.Close();
        }

        /// <summary> Return a string identifier that uniquely differentiates
        /// this Directory instance from other Directory instances.
        /// This ID should be the same if two Directory instances
        /// (even in different JVMs and/or on different machines)
        /// are considered "the same index".  This is how locking
        /// "scopes" to the right index.
        /// </summary>
        public override string GetLockID()
        {
            return string.Concat(base.GetLockID(), _cacheDirectory.GetLockID());
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
        public StreamInput OpenCachedInputAsStream(string name)
        {
            return new StreamInput(CacheDirectory.OpenInput(name));
        }

        public StreamOutput CreateCachedOutputAsStream(string name)
        {
            return new StreamOutput(CacheDirectory.CreateOutput(name));
        }


        private void CheckDirty()
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
                        var masterSeg = SegmentInfos.GetCurrentSegmentGeneration(GetAllBlobFiles());
                        var localSeg = SegmentInfos.GetCurrentSegmentGeneration(_cacheDirectory);
                        _inSync = masterSeg == localSeg && masterSeg != -1;
                        _dirty = false;
                    }
                }
            }
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
