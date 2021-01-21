using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Examine.RemoveDirectory
{
    /// <summary>
    /// A Lucene syncDirectory used to store master index files in blob storage and sync local files to a %temp% fast drive storage
    /// </summary>
    public class RemoteSyncDirectory : ExamineDirectory
    {
        private volatile bool _dirty = true;
        private bool _inSync = false;
        private readonly object _locker = new object();

        protected LockFactory _lockFactory;
        public readonly IRemoteDirectory RemoteDirectory;
        private readonly IRemoteIndexOutputFactory _remoteIndexOutputFactory;
        private readonly IRemoteDirectoryIndexInputFactory _remoteDirectoryIndexInputFactory;

        /// <summary>
        /// Create an AzureDirectory
        /// </summary>
        /// <param name="connectionString">storage account to use</param>
        /// <param name="containerName">name of container (folder in blob storage)</param>
        /// <param name="cacheDirectory">local Directory object to use for local cache</param>
        /// <param name="compressBlobs"></param>
        /// <param name="rootFolder">path of the root folder inside the container</param>
        /// <param name="isReadOnly">
        /// By default this is set to false which means that the <see cref="LockFactory"/> created for this syncDirectory will be 
        /// a <see cref="MultiIndexLockFactory"/> which will create locks in both the cache and blob storage folders.
        /// If this is set to true, the lock factory will be the default LockFactory configured for the cache directorty.
        /// </param>
        public RemoteSyncDirectory(
            IRemoteDirectory azurelper,
            Lucene.Net.Store.Directory cacheDirectory,
            bool compressBlobs = false)
        {
            CacheDirectory = cacheDirectory;
            RemoteDirectory = azurelper;
            _lockFactory = GetLockFactory();
            _remoteIndexOutputFactory = GetAzureIndexOutputFactory();
            _remoteDirectoryIndexInputFactory = GetAzureIndexInputFactory();
            GuardCacheDirectory(CacheDirectory);
            CompressBlobs = compressBlobs;
        }

        protected virtual IRemoteDirectoryIndexInputFactory GetAzureIndexInputFactory()
        {
            return new RemoteDirectoryIndexInputFactory();
        }

        protected virtual IRemoteIndexOutputFactory GetAzureIndexOutputFactory()
        {
            return new RemoteIndexOutputFactory();
        }

        protected virtual void GuardCacheDirectory(Lucene.Net.Store.Directory cacheDirectory)
        {
            if (cacheDirectory == null) throw new ArgumentNullException(nameof(cacheDirectory));
        }


        protected virtual LockFactory GetLockFactory()
        {
            return new MultiIndexLockFactory(new RemoteDirectorySimpleLockFactory(this, RemoteDirectory), CacheDirectory.LockFactory);
        }


        public string RootFolder { get; }
        public bool CompressBlobs { get; }

        public Lucene.Net.Store.Directory CacheDirectory { get; protected set; }

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
                RemoteDirectory.SyncFile(CacheDirectory, file, CompressBlobs);
            }
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
            return RemoteDirectory.GetAllRemoteFileNames();
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

                    Trace.WriteLine(
                        $"ERROR {e.ToString()}  Exception thrown while checking file ({name}) exists for {RootFolder}");
                    SetDirty();
                    return RemoteDirectory.FileExists(name);;
                }
            }

            return RemoteDirectory.FileExists(name);
        }

        /// <summary>Returns the time the named file was last modified. </summary>
        public override long FileModified(string name)
        {
            CheckDirty();

            if (_inSync)
            {
                return CacheDirectory.FileModified(name);
            }

            return RemoteDirectory.FileModified(name);
            
        }

        /// <summary>Set the modified time of an existing file to now. </summary>
        [Obsolete("This is actually never used")]
        public override void TouchFile(string name)
        {
            //just update the cache file - the Lucene source actually never calls this method!
            CacheDirectory.TouchFile(name);
            SetDirty();
        }

        /// <summary>Removes an existing file in the syncDirectory. </summary>
        public override void DeleteFile(string name)
        {
            //We're going to try to remove this from the cache syncDirectory first,
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

            //if we've made it this far then the cache directly file has been successfully removed so now we'll do the master

            RemoteDirectory.DeleteFile(name);
            SetDirty();
        }


        /// <summary>Returns the length of a file in the syncDirectory. </summary>
        public override long FileLength(string name)
        {
            CheckDirty();

            if (_inSync)
            {
                return CacheDirectory.FileLength(name);
            }

            return RemoteDirectory.FileLength(name, CacheDirectory.FileLength(name));
        }

        /// <summary>Creates a new, empty file in the syncDirectory with the given name.
        /// Returns a stream writing this file. 
        /// </summary>
        public override IndexOutput CreateOutput(string name)
        {
            SetDirty();

            return _remoteIndexOutputFactory.CreateIndexOutput(this, name);
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
                    Trace.WriteLine(
                        $"DEBUG {ex.ToString()} File {name} not found. Will need to resync for {RootFolder}");
                    SetDirty();
                }
                catch (Exception ex)
                {
                    Trace.TraceError(
                        "Could not get local file though we are marked as inSync, reverting to try blob storage; " +
                        ex);
                    Trace.WriteLine(
                        $"ERROR {ex.ToString()} Could not get local file though we are marked as inSync, reverting to try blob storage; {RootFolder}");
                }
            }

            if (RemoteDirectory.TryGetBlobFile(name))
            {
                return _remoteDirectoryIndexInputFactory.GetIndexInput(this, RemoteDirectory, name);
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
            _lockFactory.ClearLock(name);
        }

        public override LockFactory LockFactory => _lockFactory;


        protected override void Dispose(bool disposing)
        {
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


        /// <summary>
        /// Checks dirty flag and sets the _inSync flag after querying the blob strorage vs local storage segment gen
        /// </summary>
        /// <returns>
        /// If _dirty is true and blob storage files are looked up, this will return those blob storage files, this is a performance gain so
        /// we don't double query blob storage.
        /// </returns>
        public virtual string[] CheckDirty()
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


    }
}