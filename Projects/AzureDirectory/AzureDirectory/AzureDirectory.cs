//    License: Microsoft Public License (Ms-PL) 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IndexFileNameFilter = Lucene.Net.Index.IndexFileNameFilter;
using Lucene.Net;
using Lucene.Net.Store;
using Azure = Microsoft.WindowsAzure.StorageClient;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.WindowsAzure;
using System.Configuration;
using Microsoft.WindowsAzure.StorageClient;
using System.Xml.Serialization;

namespace Lucene.Net.Store.Azure
{
    public class AzureDirectory : Directory
    {
        private string _catalog;
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;
        private Directory _cacheDirectory;

        #region CTOR
        public AzureDirectory(CloudStorageAccount storageAccount) :
            this(storageAccount, null, null)
        {
        }

        /// <summary>
        /// Create AzureDirectory
        /// </summary>
        /// <param name="storageAccount">staorage account to use</param>
        /// <param name="catalog">name of catalog (folder in blob storage)</param>
        /// <remarks>Default local cache is to use file system in user/appdata/AzureDirectory/Catalog</remarks>
        public AzureDirectory(
            CloudStorageAccount storageAccount,
            string catalog)
            : this(storageAccount, catalog, null)
        {
        }

        /// <summary>
        /// Create an AzureDirectory
        /// </summary>
        /// <param name="storageAccount">storage account to use</param>
        /// <param name="catalog">name of catalog (folder in blob storage)</param>
        /// <param name="cacheDirectory">local Directory object to use for local cache</param>
        public AzureDirectory(
            CloudStorageAccount storageAccount,
            string catalog,
            Directory cacheDirectory)
        {
            if (storageAccount == null)
                throw new ArgumentNullException("storageAccount");

            if (string.IsNullOrEmpty(catalog))
                _catalog = "lucene";
            else
                _catalog = catalog.ToLower();

            _blobClient = storageAccount.CreateCloudBlobClient();
            _initCacheDirectory(cacheDirectory);
        }

        public CloudBlobContainer BlobContainer
        {
            get
            {
                return _blobContainer;
            }
        }

#if COMPRESSBLOBS
        public bool CompressBlobs
        {
            get;
            set;
        }
#endif
        public void ClearCache()
        {
            foreach (string file in _cacheDirectory.List())
            {
                _cacheDirectory.DeleteFile(file);
            }
        }

        public Directory CacheDirectory
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
        #endregion

        #region internal methods
        private void _initCacheDirectory(Directory cacheDirectory)
        {
#if COMPRESSBLOBS
            CompressBlobs = true;
#endif
            if (cacheDirectory != null)
            {
                // save it off
                _cacheDirectory = cacheDirectory;
            }
            else
            {
                string cachePath = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "AzureDirectory");
                System.IO.DirectoryInfo azureDir = new System.IO.DirectoryInfo(cachePath);
                if (!azureDir.Exists)
                    azureDir.Create();

                string catalogPath = System.IO.Path.Combine(cachePath, _catalog);
                System.IO.DirectoryInfo catalogDir = new System.IO.DirectoryInfo(catalogPath);
                if (!catalogDir.Exists)
                    catalogDir.Create();

                _cacheDirectory = FSDirectory.GetDirectory(catalogPath);
            }

            CreateContainer();
        }

        public void CreateContainer()
        {
            _blobContainer = _blobClient.GetContainerReference(_catalog);

            // create it if it does not exist
            _blobContainer.CreateIfNotExist();
        }
        #endregion

        #region DIRECTORY METHODS
        /// <summary>Returns an array of strings, one for each file in the directory. </summary>
        public override System.String[] List()
        {
            var results = from blob in _blobContainer.ListBlobs()
                          select blob.Uri.AbsolutePath.Substring(blob.Uri.AbsolutePath.LastIndexOf('/') + 1);
            return results.ToArray<string>();
        }

        /// <summary>Returns true if a file with the given name exists. </summary>
        public override bool FileExists(System.String name)
        {
            // this always comes from the server
            try
            {
                var blob = _blobContainer.GetBlobReference(name);
                blob.FetchAttributes();
                return true;
            }
            catch(StorageClientException)
            {
                return false;
            }
        }

        /// <summary>Returns the time the named file was last modified. </summary>
        public override long FileModified(System.String name)
        {
            // this always has to come from the server
            try
            {
                var blob = _blobContainer.GetBlobReference(name);
                blob.FetchAttributes();
                return blob.Properties.LastModifiedUtc.ToFileTimeUtc();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>Set the modified time of an existing file to now. </summary>
        public override void TouchFile(System.String name)
        {
            //BlobProperties props = _blobContainer.GetBlobProperties(name);
            //_blobContainer.UpdateBlobMetadata(props);
            // I have no idea what the semantics of this should be...hmmmm...
            // we never seem to get called
            _cacheDirectory.TouchFile(name);
            //SetCachedBlobProperties(props);
        }

        /// <summary>Removes an existing file in the directory. </summary>
        public override void DeleteFile(System.String name)
        {
            var blob = _blobContainer.GetBlobReference(name);
            blob.DeleteIfExists();
            Debug.WriteLine(String.Format("DELETE {0}/{1}", _blobContainer.Uri.ToString(), name));

            if (_cacheDirectory.FileExists(name + ".blob"))
                _cacheDirectory.DeleteFile(name + ".blob");

            if (_cacheDirectory.FileExists(name))
                _cacheDirectory.DeleteFile(name);
        }

        /// <summary>Renames an existing file in the directory.
        /// If a file already exists with the new name, then it is replaced.
        /// This replacement should be atomic. 
        /// </summary>
        public override void RenameFile(System.String from, System.String to)
        {
            try
            {
                var blobFrom = _blobContainer.GetBlobReference(from);
                var blobTo = _blobContainer.GetBlobReference(to);
                blobTo.CopyFromBlob(blobFrom);
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
        public override long FileLength(System.String name)
        {
            var blob = _blobContainer.GetBlobReference(name);
            blob.FetchAttributes();

            // index files may be compressed so the actual length is stored in metatdata
            long blobLength;
            if (long.TryParse(blob.Metadata["CachedLength"], out blobLength))
                return blobLength;
            else
                return blob.Properties.Length; // fall back to actual blob size
        }

        /// <summary>Creates a new, empty file in the directory with the given name.
        /// Returns a stream writing this file. 
        /// </summary>
        public override IndexOutput CreateOutput(System.String name)
        {
            var blob = _blobContainer.GetBlobReference(name);
            return new AzureIndexOutput(this, blob);
        }

        /// <summary>Returns a stream reading an existing file. </summary>
        public override IndexInput OpenInput(System.String name)
        {
            try
            {
                CloudBlob blob = _blobContainer.GetBlobReference(name);
                blob.FetchAttributes();
                AzureIndexInput input = new AzureIndexInput(this, blob);
                return input;
            }
            catch (Exception err)
            {
                throw new System.IO.FileNotFoundException(name, err);
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
                    _locks.Add(name, new AzureLock(name, this));
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

        public override void Dispose()
        {
            //TODO: This is a new implementation of Lucene, do we need to do something here?
        }

        #endregion

        #region Azure specific methods
#if COMPRESSBLOBS
        public virtual bool ShouldCompressFile(string path)
        {
            if (!CompressBlobs)
                return false;

            string ext = System.IO.Path.GetExtension(path);
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
#endif
        public StreamInput OpenCachedInputAsStream(string name)
        {
            return new StreamInput(CacheDirectory.OpenInput(name));
        }

        public StreamOutput CreateCachedOutputAsStream(string name)
        {
            return new StreamOutput(CacheDirectory.CreateOutput(name));
        }

        #endregion
    }

}
