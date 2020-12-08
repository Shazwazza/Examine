using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Examine.AzureDirectory
{

    /// <summary>
    /// A Lucene directory used to store master index files in blob storage and sync local files to a %temp% fast drive storage
    /// </summary>
    public class AzureLuceneDirectory : ExamineDirectory
    {
        private readonly string _storageAccountConnectionString;
        private volatile bool _dirty = true;
        private bool _inSync = false;
        private readonly object _locker = new object();

        private readonly string _containerName;
        protected BlobContainerClient _blobContainer;
        protected LockFactory _lockFactory;
        private static readonly NoopIndexOutput _noopIndexOutput = new NoopIndexOutput();
        private readonly IAzureIndexOutputFactory _azureIndexOutputFactory;
        private readonly IAzureIndexInputFactory _azureIndexInputFactory;

        /// <summary>
        /// Create an AzureDirectory
        /// </summary>
        /// <param name="connectionString">storage account to use</param>
        /// <param name="containerName">name of container (folder in blob storage)</param>
        /// <param name="cacheDirectory">local Directory object to use for local cache</param>
        /// <param name="compressBlobs"></param>
        /// <param name="rootFolder">path of the root folder inside the container</param>
        /// <param name="isReadOnly">
        /// By default this is set to false which means that the <see cref="LockFactory"/> created for this directory will be 
        /// a <see cref="MultiIndexLockFactory"/> which will create locks in both the cache and blob storage folders.
        /// If this is set to true, the lock factory will be the default LockFactory configured for the cache directorty.
        /// </param>
        public AzureLuceneDirectory(
            string connectionString,
            string containerName,
            Lucene.Net.Store.Directory cacheDirectory,
            bool compressBlobs = false,
            string rootFolder = null,
            bool isReadOnly = false)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(containerName));
            IsReadOnly = isReadOnly;
            _storageAccountConnectionString = connectionString;
            CacheDirectory = cacheDirectory;
            _containerName = containerName.ToLower();
            _lockFactory = GetLockFactory();
            RootFolder = NormalizeContainerRootFolder(rootFolder);

            EnsureContainer();
            _azureIndexOutputFactory = GetAzureIndexOutputFactory();
            _azureIndexInputFactory = GetAzureIndexInputFactory();
            GuardCacheDirectory(CacheDirectory);
            CompressBlobs = compressBlobs;
        }
        protected virtual IAzureIndexInputFactory GetAzureIndexInputFactory()
        {
            return new AzureIndexInputFactory();
        }
        protected virtual IAzureIndexOutputFactory GetAzureIndexOutputFactory()
        {
            return new AzureIndexOutputFactory();
        }

        protected virtual void GuardCacheDirectory(Lucene.Net.Store.Directory cacheDirectory)
        {
            if (cacheDirectory == null) throw new ArgumentNullException(nameof(cacheDirectory));
        }

        protected string NormalizeContainerRootFolder(string rootFolder)
        {
            if (string.IsNullOrEmpty(rootFolder))
                return string.Empty;
            rootFolder = rootFolder.Trim('/');
            rootFolder = rootFolder + "/";
            return rootFolder;
        }

        protected virtual LockFactory GetLockFactory()
        {
            return IsReadOnly ? (LockFactory)new NoopLockFactory()
                : new MultiIndexLockFactory(new AzureDirectorySimpleLockFactory(this), CacheDirectory.LockFactory);
        }
        protected virtual BlobClient GetBlobClient(string blobName)
        {
            return new BlobClient(_storageAccountConnectionString, _containerName, blobName);
        }
        protected virtual BlobContainerClient GetBlobContainerClient(string containerName)
        {
            return new BlobContainerClient(_storageAccountConnectionString, _containerName);
        }

        public string RootFolder { get; }
        public bool CompressBlobs { get; }

        public void ClearCache()
        {
            Trace.WriteLine($"Clearing index cache {RootFolder}");
            foreach (string file in CacheDirectory.ListAll())
            {
                Trace.WriteLine("DEBUG Deleting cache file {file}", file);
                CacheDirectory.DeleteFile(file);
            }
        }
        public virtual void RebuildCache()
        {
            Trace.WriteLine($"INFO Rebuilding index cache {RootFolder}");
            try
            {
                ClearCache();
            }
            catch (Exception e)
            {
                Trace.WriteLine($"ERROR {e.ToString()}  Exception thrown while rebuilding cache for {RootFolder}");
            }
            foreach (string file in GetAllBlobFiles())
            {
                CacheDirectory.TouchFile(file);
                var blob = GetBlobClient(RootFolder + file);
                SyncFile(blob, file);
            }
        }

        public virtual void EnsureContainer()
        {
            Trace.WriteLine($"DEBUG Ensuring container ({_containerName}) exists for cache {RootFolder}");
            _blobContainer = GetBlobContainerClient(_containerName);
            _blobContainer.CreateIfNotExists();
        }

        public override string[] ListAll()
        {
            var blobFiles = CheckDirty();

            return _inSync
                ? CacheDirectory.ListAll()
                : (blobFiles ?? GetAllBlobFiles());
        }

        internal string[] GetAllBlobFiles()
        {
            IEnumerable<string> results = GetAllBlobFileNames();
            if (string.IsNullOrWhiteSpace(RootFolder))
            {
                return results.ToArray();
            }
            var names = results.Where(x => !x.EndsWith(".lock")).Select(x => x.Replace(RootFolder, "")).ToArray();
            return names;
        }

        protected virtual IEnumerable<string> GetAllBlobFileNames()
        {
            return from blob in _blobContainer.GetBlobs(prefix: RootFolder)
                   select blob.Name;
        }

        /// <summary>Returns true if a file with the given name exists. </summary>
        public override bool FileExists(string name)
        {
            CheckDirty();

            if (_inSync)
            {
                try
                {
                    return CacheDirectory.FileExists(name);
                }
                catch (Exception e)
                {
                    // something isn't quite right, need to re-sync

                    Trace.WriteLine($"ERROR {e.ToString()}  Exception thrown while checking file ({name}) exists for {RootFolder}");
                    SetDirty();
                    return BlobExists(name);
                }
            }

            return BlobExists(name);
        }

        /// <summary>Returns the time the named file was last modified. </summary>
        public override long FileModified(string name)
        {
            CheckDirty();

            if (_inSync)
            {
                return CacheDirectory.FileModified(name);
            }

            if (TryGetBlobFile(name, out var blob, out var err))
            {
                var blobPropertiesResponse = blob.GetProperties();
                var blobProperties = blobPropertiesResponse.Value;
                if (blobProperties.LastModified != null)
                {
                    var utcDate = blobProperties.LastModified.UtcDateTime;

                    //This is the data structure of how the default Lucene FSDirectory returns this value so we want
                    // to be consistent with how Lucene works
                    return (long)utcDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
                }

                // TODO: Need to check lucene source, returning this value could be problematic
                return 0;
            }
            else
            {
                Trace.WriteLine($"WARNING Throwing exception as blob file ({name}) not found for {RootFolder}");
                // Lucene expects this exception to be thrown
                throw new FileNotFoundException(name, err);
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
        protected void SyncFile(BlobClient _blob, string fileName)
        {
            Trace.WriteLine($"INFO Syncing file {fileName} for {RootFolder}");
            // then we will get it fresh into local deflatedName 
            // StreamOutput deflatedStream = new StreamOutput(CacheDirectory.CreateOutput(deflatedName));
            using (var deflatedStream = new MemoryStream())
            {
                // get the deflated blob
                _blob.DownloadTo(deflatedStream);

#if FULLDEBUG
                Trace.WriteLine($"GET {fileName} RETREIVED {deflatedStream.Length} bytes");
#endif 

                // seek back to begininng
                deflatedStream.Seek(0, SeekOrigin.Begin);

                if (ShouldCompressFile(fileName))
                {
                    // open output file for uncompressed contents
                    using (var fileStream = new StreamOutput(CacheDirectory.CreateOutput(fileName)))
                    using (var decompressor = new DeflateStream(deflatedStream, CompressionMode.Decompress))
                    {
                        var bytes = new byte[65535];
                        var nRead = 0;
                        do
                        {
                            nRead = decompressor.Read(bytes, 0, 65535);
                            if (nRead > 0)
                                fileStream.Write(bytes, 0, nRead);
                        } while (nRead == 65535);
                    }
                }
                else
                {
                    using (var fileStream = new StreamOutput(CacheDirectory.CreateOutput(fileName)))
                    {
                        // get the blob
                        _blob.DownloadTo(fileStream);

                        fileStream.Flush();
#if FULLDEBUG
                        Trace.WriteLine($"GET {fileName} RETREIVED {fileStream.Length} bytes");
#endif
                    }
                }

            }
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

                Trace.WriteLine($"ERROR {ex.ToString()} Exception thrown while deleting file {name} for {RootFolder}");
                throw;
            }

            //if we are readonly, then we are only modifying local storage
            if (IsReadOnly) return;

            //if we've made it this far then the cache directly file has been successfully removed so now we'll do the master

            var blob = GetBlobClient(RootFolder + name);
            blob.DeleteIfExists();
            SetDirty();

            Trace.WriteLine($"INFO Deleted { _blobContainer.Uri}/{name} for {RootFolder}");
            Trace.WriteLine($"INFO DELETE {_blobContainer.Uri}/{name}");
        }


        /// <summary>Returns the length of a file in the directory. </summary>
        public override long FileLength(string name)
        {
            CheckDirty();

            if (_inSync)
            {
                return CacheDirectory.FileLength(name);
            }

            try
            {
                var blob = GetBlobClient(RootFolder + name);
                var blobProperties = blob.GetProperties();

                // index files may be compressed so the actual length is stored in metadata
                var hasMetadataValue = blobProperties.Value.Metadata.TryGetValue("CachedLength", out var blobLegthMetadata);

                if (hasMetadataValue && long.TryParse(blobLegthMetadata, out var blobLength))
                {
                    return blobLength;
                }

                // fall back to actual blob size
                return blobProperties.Value.ContentLength;
            }
            catch (Exception e)
            {
                //  Sync(name);
                Trace.WriteLine($"ERROR {e.ToString()}  Exception thrown while retrieving file length of file {name} for {RootFolder}");
                return CacheDirectory.FileLength(name);
            }
        }

        public override void Sync(string name)
        {
            if (IsReadOnly)
            {
                var allBlobs = GetAllBlobFiles();
                foreach (var toCheck in CacheDirectory.ListAll())
                {
                    if (allBlobs.Contains(toCheck))
                    {
                        continue;
                    }

                    try
                    {
                        RebuildCache();
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine($"ERROR {e.ToString()}  Exception thrown while syncing {name} for {RootFolder}");
                    }
                }
            }
            else
            {
                base.Sync(name);
            }
        }

        /// <summary>Creates a new, empty file in the directory with the given name.
        /// Returns a stream writing this file. 
        /// </summary>
        public override IndexOutput CreateOutput(string name)
        {
            SetDirty();

            //if we are readonly, then we don't modify anything
            if (IsReadOnly)
            {
                return _noopIndexOutput;
            }

            var blob = _blobContainer.GetBlobClient(RootFolder + name);
            return _azureIndexOutputFactory.CreateIndexOutput(this, blob, name);
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
                catch (FileNotFoundException ex)
                {
                    //if it's not found then we need to re-read from blob so were not in sync
                    Trace.WriteLine($"DEBUG {ex.ToString()} File {name} not found. Will need to resync for {RootFolder}");
                    SetDirty();
                }
                catch (Exception ex)
                {
                    Trace.TraceError(
                        "Could not get local file though we are marked as inSync, reverting to try blob storage; " +
                        ex);
                    Trace.WriteLine($"ERROR {ex.ToString()} Could not get local file though we are marked as inSync, reverting to try blob storage; {RootFolder}");
                }
            }

            if (TryGetBlobFile(name, out var blob, out var err))
            {
                return _azureIndexInputFactory.GetIndexInput(this, blob);
            }
            else
            {
                SetDirty();
                return CacheDirectory.OpenInput(name);
                //   throw new FileNotFoundException(name, err);
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
            if (!IsReadOnly)
            {
                _lockFactory.ClearLock(name);
            }

            CacheDirectory.ClearLock(name);
        }

        public override LockFactory LockFactory => _lockFactory;

        public BlobContainerClient BlobContainer { get => _blobContainer; set => _blobContainer = value; }

        protected override void Dispose(bool disposing)
        {
            _blobContainer = null;
            CacheDirectory?.Dispose();
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
            }

            ;
        }

        /// <summary>
        /// Checks dirty flag and sets the _inSync flag after querying the blob strorage vs local storage segment gen
        /// </summary>
        /// <returns>
        /// If _dirty is true and blob storage files are looked up, this will return those blob storage files, this is a performance gain so
        /// we don't double query blob storage.
        /// </returns>
        public string[] CheckDirty()
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
                        if (!_inSync)
                        {
                            HandleOutOfSync();
                        }
                        _dirty = false;
                        return blobFiles;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Checks dirty flag and sets the _inSync flag after querying the blob strorage vs local storage segment gen
        /// </summary>
        /// <returns>
        /// If _dirty is true and blob storage files are looked up, this will return those blob storage files, this is a performance gain so
        /// we don't double query blob storage.
        /// </returns>
        public override string[] CheckDirtyWithoutWriter()
        {
            return CheckDirty();
        }

        /// <summary>
        /// Called when the index is out of sync with the master index
        /// </summary>
        protected virtual void HandleOutOfSync()
        {
            //Do nothing
        }

        public override void SetDirty()
        {
            if (!_dirty)
            {
                lock (_locker)
                {
                    _dirty = true;
                }
            }
        }

        private bool BlobExists(string name)
        {
            try
            {
                var client = _blobContainer.GetBlobClient(RootFolder + name);
                return client.Exists().Value;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"WARNING {ex.ToString()} Exception thrown while checking blob ({name}) exists. Assuming blob does not exist for {RootFolder}");
                return false;
            }
        }

        private bool TryGetBlobFile(string name, out BlobClient blob, out RequestFailedException err)
        {
            try
            {
                blob = _blobContainer.GetBlobClient(RootFolder + name);
                var properties = blob.GetProperties();
                err = null;
                return true;
            }
            catch (RequestFailedException e)
            {
                Trace.WriteLine($"ERROR {e.ToString()}  Exception thrown while trying to retrieve blob ({name}). Assuming blob does not exist for {RootFolder}");
                err = e;
                blob = null;
                return false;
            }
        }

        #region Sync
        /// <summary>Creates a new, empty file in the directory with the given name.
        /// Returns a stream writing this file. 
        /// </summary>
        public IndexOutput CreateOutput(string blobFileName, string name)
        {
            SetDirty();

            //if we are readonly, then we don't modify anything
            if (IsReadOnly)
            {
                return _noopIndexOutput;
            }

            var blob = _blobContainer.GetBlobClient(GenerateBlobName(blobFileName));
            return _azureIndexOutputFactory.CreateIndexOutput(this, blob, name);
        }

        private string GenerateBlobName(string blobFileName)
        {
            return RootFolder + blobFileName;
        }
        private string CleanBlobName(string blobFileName)
        {
            return blobFileName.Replace(RootFolder, "");
        }
        protected override void CleanupRemoteFiles()
        {
            //TODO : Get all manifests on remote. Find older than x. Remove files older than x not referenced by a newer manifest, delete old manifest
            List<ExamineDirectoryManifest> manifests = GetAllManifests();
            var orderedManifests = manifests.OrderByDescending(x => x.Modified);
            var retainedManifests = orderedManifests.Where((x, y) => y < 2 || !ManifestExpired(x));
            var removableManifests = orderedManifests.Where(x => !retainedManifests.Contains(x));
            var retainedFiles = removableManifests.SelectMany(x => x.Entries).Select(x => x.BlobFileName).ToDictionary(x => x, y => y);
            foreach (var manifest in removableManifests)
            {
                foreach (var entry in manifest.Entries)
                {
                    if (!retainedFiles.ContainsKey(entry.BlobFileName))
                    {
                        _blobContainer.DeleteBlobIfExists(GenerateBlobName(entry.BlobFileName));
                    }
                }
                _blobContainer.DeleteBlobIfExists(GenerateBlobName(GenerateManifestFileName(manifest)));
            }
        }

        protected virtual bool ManifestExpired(ExamineDirectoryManifest manifest)
        {
            //Suggest tracking sync state
           return manifest.Modified < DateTime.Now.AddHours(1).Ticks;
        }
        protected override void UploadToRemote(ExamineDirectoryManifest manifest)
        {
            foreach (var item in manifest.Entries)
            {
                if (item.OriginalManifestId == manifest.Id || !BlobExists(item.BlobFileName))
                {
                    //New/Updated/missing file
                    CreateOutput(item.BlobFileName, item.LuceneFileName);
                }
            }

            var jsonString = SerializeManifest(manifest);
            using (var stream = new MemoryStream())
            {
                var sw = new StreamWriter(stream);
                sw.Write(jsonString);
                sw.Flush();
                stream.Position = 0;
                var client = _blobContainer.GetBlobClient(GenerateBlobName( GenerateManifestFileName(manifest)));
                client.Upload(stream);
            }
        }

        private string GenerateManifestFileName(ExamineDirectoryManifest manifest)
        {
            return MANIFEST_FILE_PREFIX + manifest.Id + MANIFEST_FILE_EXTENSION;
        }

        public const string MANIFEST_FILE_PREFIX = "cc-";
        public const string MANIFEST_FILE_EXTENSION = ".manifest";
        protected override ExamineDirectoryManifest GetMostRecentManifest()
        {
            List<ExamineDirectoryManifest> manifests = GetAllManifests();
            return manifests.OrderByDescending(x => x.Modified).FirstOrDefault();
        }

        protected override List<ExamineDirectoryManifest> GetAllManifests()
        {
            List<ExamineDirectoryManifest> manifests = new List<ExamineDirectoryManifest>();
            foreach (var blob in _blobContainer.GetBlobs(prefix: RootFolder + MANIFEST_FILE_PREFIX))
            {
                try
                {
                    if (!blob.Name.EndsWith(MANIFEST_FILE_EXTENSION))
                    {
                        continue;
                    }
                    var client = _blobContainer.GetBlobClient(blob.Name);
                    using (var ms = new MemoryStream())
                    {
                        client.DownloadTo(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        using (StreamReader reader = new StreamReader(ms))
                        {
                            string text = reader.ReadToEnd();
                            var manifest = DeserializeManifest(text);
                            manifests.Add(manifest);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"ERROR Failed to download manifest {blob}. {ex.ToString()}");
                }
            }

            return manifests;
        }

        public override string SerializeManifest(ExamineDirectoryManifest manifest)
        {
            return JsonSerializer.Serialize(manifest);
        }
        public override ExamineDirectoryManifest DeserializeManifest(string manifestText)
        {
            return JsonSerializer.Deserialize<ExamineDirectoryManifest>(manifestText);
        }

        #endregion
    }
}
