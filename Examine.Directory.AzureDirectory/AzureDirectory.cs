using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Examine.Directory.Sync;
using Lucene.Net.Store;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Examine.Directory.AzureDirectory
{
    public class AzureDirectory : Lucene.Net.Store.Directory
    {
        private string _containerName;
        private string _rootFolder;
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;
        private Lucene.Net.Store.Directory _cacheDirectory;

        /// <summary>
        /// Create an AzureDirectory
        /// </summary>
        /// <param name="storageAccount">storage account to use</param>
        /// <param name="containerName">name of container (folder in blob storage)</param>
        /// <param name="cacheDirectory">local Directory object to use for local cache</param>
        /// <param name="rootFolder">path of the root folder inside the container</param>
        public AzureDirectory(
            CloudStorageAccount storageAccount,
            string containerName = null,
            Lucene.Net.Store.Directory cacheDirectory = null,
            bool compressBlobs = false,
            string rootFolder = null)
        {
            if (storageAccount == null)
                throw new ArgumentNullException("storageAccount");

            if (string.IsNullOrEmpty(containerName))
                _containerName = "lucene";
            else
                _containerName = containerName.ToLower();


            if (string.IsNullOrEmpty(rootFolder))
                _rootFolder = string.Empty;
            else
            {
                rootFolder = rootFolder.Trim('/');
                _rootFolder = rootFolder + "/";
            }


            _blobClient = storageAccount.CreateCloudBlobClient();
            _initCacheDirectory(cacheDirectory);
            this.CompressBlobs = compressBlobs;
        }

        public CloudBlobContainer BlobContainer
        {
            get
            {
                return _blobContainer;
            }
        }

        public bool CompressBlobs
        {
            get;
            set;
        }

        public void ClearCache()
        {
            foreach (string file in _cacheDirectory.ListAll())
            {
                _cacheDirectory.DeleteFile(file);
            }
        }

        public Lucene.Net.Store.Directory CacheDirectory
        {
            get
            {
                return _cacheDirectory;
            }
            set
            {
                _cacheDirectory = value;
            }
        }

        private void _initCacheDirectory(Lucene.Net.Store.Directory cacheDirectory)
        {
            if (cacheDirectory != null)
            {
                // save it off
                _cacheDirectory = cacheDirectory;
            }
            else
            {
                var cachePath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "AzureDirectory");
                var azureDir = new DirectoryInfo(cachePath);
                if (!azureDir.Exists)
                    azureDir.Create();

                var catalogPath = Path.Combine(cachePath, _containerName);
                var catalogDir = new DirectoryInfo(catalogPath);
                if (!catalogDir.Exists)
                    catalogDir.Create();

                _cacheDirectory = FSDirectory.Open(catalogDir);
            }

            CreateContainer();
        }

        public void CreateContainer()
        {
            _blobContainer = _blobClient.GetContainerReference(_containerName);
            _blobContainer.CreateIfNotExists();
        }

        /// <summary>Returns an array of strings, one for each file in the directory. </summary>
        [Obsolete("For some Directory implementations (FSDirectory}, and its subclasses), this method silently filters its results to include only index files.  Please use ListAll instead, which does no filtering. ")]
        public override String[] List()
        {
            var results = from blob in _blobContainer.ListBlobs(_rootFolder)
                          select blob.Uri.AbsolutePath.Substring(blob.Uri.AbsolutePath.LastIndexOf('/') + 1);
            return results.ToArray<string>();
        }

        /// <summary>Returns true if a file with the given name exists. </summary>
        public override bool FileExists(String name)
        {
            // this always comes from the server
            try
            {
                return _blobContainer.GetBlockBlobReference(_rootFolder + name).Exists();
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Returns the time the named file was last modified. </summary>
        public override long FileModified(String name)
        {
            // this always has to come from the server
            try
            {
                var blob = _blobContainer.GetBlockBlobReference(_rootFolder + name);
                blob.FetchAttributes();
                return blob.Properties.LastModified.Value.UtcDateTime.ToFileTimeUtc();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>Set the modified time of an existing file to now. </summary>
        public override void TouchFile(System.String name)
        {
            //BlobProperties props = _blobContainer.GetBlobProperties(_rootFolder + name);
            //_blobContainer.UpdateBlobMetadata(props);
            // I have no idea what the semantics of this should be...hmmmm...
            // we never seem to get called
            _cacheDirectory.TouchFile(name);
            //SetCachedBlobProperties(props);
        }

        /// <summary>Removes an existing file in the directory. </summary>
        public override void DeleteFile(System.String name)
        {
            var blob = _blobContainer.GetBlockBlobReference(_rootFolder + name);
            blob.DeleteIfExists();
            Debug.WriteLine(String.Format("DELETE {0}/{1}", _blobContainer.Uri.ToString(), name));

            if (_cacheDirectory.FileExists(name + ".blob"))
            {
                _cacheDirectory.DeleteFile(name + ".blob");
            }

            if (_cacheDirectory.FileExists(name))
            {
                _cacheDirectory.DeleteFile(name);
            }
        }

        
        /// <summary>Renames an existing file in the directory.
        /// If a file already exists with the new name, then it is replaced.
        /// This replacement should be atomic. 
        /// </summary>
        [Obsolete]
        public override void RenameFile(System.String from, System.String to)
        {
            try
            {
                var blobFrom = _blobContainer.GetBlockBlobReference(from);
                var blobTo = _blobContainer.GetBlockBlobReference(to);
                blobTo.StartCopyFromBlob(blobFrom);
                blobFrom.DeleteIfExists();

                // we delete and force a redownload, since we can't do this in an atomic way
                if (_cacheDirectory.FileExists(from))
                    _cacheDirectory.RenameFile(from, to);

                // drop old cached data as it's wrong now
                if (_cacheDirectory.FileExists(from + ".blob"))
                    _cacheDirectory.DeleteFile(from + ".blob");
            }
            catch
            {
            }
        }

        /// <summary>Returns the length of a file in the directory. </summary>
        public override long FileLength(String name)
        {
            var blob = _blobContainer.GetBlockBlobReference(_rootFolder + name);
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
        public override IndexOutput CreateOutput(System.String name)
        {
            var blob = _blobContainer.GetBlockBlobReference(_rootFolder + name);
            return new AzureIndexOutput(this, blob);
        }

        /// <summary>Returns a stream reading an existing file. </summary>
        public override IndexInput OpenInput(System.String name)
        {
            try
            {
                var blob = _blobContainer.GetBlockBlobReference(_rootFolder + name);
                blob.FetchAttributes();
                return new AzureIndexInput(this, blob);
            }
            catch (Exception err)
            {
                throw new FileNotFoundException(name, err);
            }
        }

        private Dictionary<string, AzureLock> _locks = new Dictionary<string, AzureLock>();

        /// <summary>Construct a {@link Lock}.</summary>
        /// <param name="name">the name of the lock file
        /// </param>
        public override Lock MakeLock(System.String name)
        {
            lock (_locks)
            {
                if (!_locks.ContainsKey(name))
                {
                    _locks.Add(name, new AzureLock(_rootFolder + name, this));
                }
                return _locks[name];
            }
        }

        public override void ClearLock(string name)
        {
            lock (_locks)
            {
                if (_locks.ContainsKey(name))
                {
                    _locks[name].BreakLock();
                }
            }
            _cacheDirectory.ClearLock(name);
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

    }

}
