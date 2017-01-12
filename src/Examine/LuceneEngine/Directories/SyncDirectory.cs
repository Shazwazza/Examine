using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Directories
{
    [SecurityCritical]
    public class SyncDirectory : Lucene.Net.Store.Directory
    {
        private readonly Lucene.Net.Store.Directory _masterDirectory;
        private readonly Lucene.Net.Store.Directory _cacheDirectory;
        private readonly MultiIndexLockFactory _lockFactory;
        private volatile bool _dirty = true;
        private bool _inSync = false;
        private readonly object _locker = new object();


        /// <summary>
        /// Create a SyncDirectory
        /// </summary>
        /// <param name="masterDirectory"></param>
        /// <param name="cacheDirectory">local Directory object to use for local cache</param>
        public SyncDirectory(
            Lucene.Net.Store.Directory masterDirectory,
            Lucene.Net.Store.Directory cacheDirectory)
        {
            if (masterDirectory == null) throw new ArgumentNullException(nameof(masterDirectory));
            if (cacheDirectory == null) throw new ArgumentNullException(nameof(cacheDirectory));
            _masterDirectory = masterDirectory;
            _cacheDirectory = cacheDirectory;
            _lockFactory = new MultiIndexLockFactory(_masterDirectory, _cacheDirectory);
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
        }

        public Lucene.Net.Store.Directory MasterDirectory
        {
            get
            {
                return _masterDirectory;
            }
        }

        /// <summary>Returns an array of strings, one for each file in the directory. </summary>
        [Obsolete("For some Directory implementations (FSDirectory}, and its subclasses), this method silently filters its results to include only index files.  Please use ListAll instead, which does no filtering. ")]
        [SecurityCritical]
        public override String[] List()
        {
            //proxy to the non obsolete ListAll
            return ListAll();
        }

        /// <summary>Returns an array of strings, one for each file in the
        /// directory.  Unlike <see cref="M:Lucene.Net.Store.Directory.List" /> this method does no
        /// filtering of the contents in a directory, and it will
        /// never return null (throws <see cref="NoSuchDirectoryException"/> instead).
        /// </summary>
        /// <exception cref="NoSuchDirectoryException">
        /// This will throw a <see cref="NoSuchDirectoryException"/> which is expected by lucene when the directory doesn't exist yet
        /// </exception>
        /// <remarks>
        /// Since the master directory is "Slow" we don't want to always list all from the master since ListAll is used in various
        /// scenarios like: IndexReader.IsCurrent and IndexReader.IndexExists. IsCurrent is used very often to check if the reader needs
        /// to be refreshed especially when an NRT reader is not being used (i.e. when no writing has been initialized) So what if we could 
        /// figure out a 'fast' way to check if the 2 directories are out of sync and when they are, we read from the master but when they 
        /// are in sync we read from the local disk. We could use this same logic for the FileExists, FileModified, FileLength methods too.
        /// A proposal would be to:
        /// * track when the master/local is dirty and set a dirty flag
        /// * when this flag is set and one of these methods is called, we need to re-calculate the hash (or whatever) of if these dirs are in sync
        /// * when the hash matches and the dirty flag is null, for these methods we'll use the local disk
        /// </remarks>
        [SecurityCritical]
        public override string[] ListAll()
        {
            CheckDirty();

            return _inSync ? _cacheDirectory.ListAll() : _masterDirectory.ListAll();
        }

        /// <summary>Returns true if a file with the given name exists. </summary>
        [SecurityCritical]
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
                        return _masterDirectory.FileExists(name);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            else
            {
                try
                {
                    return _masterDirectory.FileExists(name);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>Returns the time the named file was last modified. </summary>
        [SecurityCritical]
        public override long FileModified(String name)
        {
            CheckDirty();

            return _inSync ? _cacheDirectory.FileModified(name) : _masterDirectory.FileModified(name);            
        }

        /// <summary>Set the modified time of an existing file to now. </summary>
        [Obsolete("This is actually never used")]
        [SecurityCritical]
        public override void TouchFile(System.String name)
        {
            //just update the cache file - the Lucene source actually never calls this method!
            _cacheDirectory.TouchFile(name);
            SetDirty();
        }

        /// <summary>Removes an existing file in the directory. </summary>
        [SecurityCritical]
        public override void DeleteFile(System.String name)
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
            _masterDirectory.DeleteFile(name);
            SetDirty();
        }


        /// <summary>Renames an existing file in the directory.
        /// If a file already exists with the new name, then it is replaced.
        /// This replacement should be atomic. 
        /// </summary>
        [Obsolete("This is actually never used")]
        [SecurityCritical]
        public override void RenameFile(System.String from, System.String to)
        {
            try
            {
                _masterDirectory.RenameFile(from, to);
                SetDirty();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not rename file on master index; " + ex);
            }

            try
            {
                // we delete and force a redownload, since we can't do this in an atomic way
                if (_cacheDirectory.FileExists(from))
                {
                    _cacheDirectory.RenameFile(from, to);
                    SetDirty();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not rename file on local index; " + ex);
            }
        }

        /// <summary>Returns the length of a file in the directory. </summary>
        [SecurityCritical]
        public override long FileLength(String name)
        {
            CheckDirty();

            return _inSync ? _cacheDirectory.FileLength(name) : _masterDirectory.FileLength(name);
        }

        /// <summary>Creates a new, empty file in the directory with the given name.
        /// Returns a stream writing this file. 
        /// </summary>
        [SecurityCritical]
        public override IndexOutput CreateOutput(System.String name)
        {
            SetDirty();
            return new SyncIndexOutput(this, name);
        }

        /// <summary>Returns a stream reading an existing file. </summary>
        [SecurityCritical]
        public override IndexInput OpenInput(System.String name)
        {
            try
            {
                return new SyncIndexInput(this, name);
            }
            catch (Exception err)
            {
                throw new FileNotFoundException(name, err);
            }
        }

        /// <summary>Construct a {@link Lock}.</summary>
        /// <param name="name">the name of the lock file
        /// </param>
        [SecurityCritical]
        public override Lock MakeLock(System.String name)
        {
            return _lockFactory.MakeLock(name);
        }

        [SecurityCritical]
        public override void ClearLock(string name)
        {
            _lockFactory.ClearLock(name);
        }

        [SecurityCritical]
        public override LockFactory GetLockFactory()
        {
            return _lockFactory;
        }

        /// <summary>
        /// Return a string identifier that uniquely differentiates
        ///             this Directory instance from other Directory instances.
        ///             This ID should be the same if two Directory instances
        ///             (even in different JVMs and/or on different machines)
        ///             are considered "the same index".  This is how locking
        ///             "scopes" to the right index.
        /// 
        /// </summary>
        [SecurityCritical]
        public override string GetLockID()
        {
            return string.Concat(_masterDirectory.GetLockID(), _cacheDirectory.GetLockID());
        }

        /// <summary>Closes the store. </summary>
        [SecurityCritical]
        public override void Close()
        {
            _masterDirectory.Close();
            _cacheDirectory.Close();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        [SecuritySafeCritical]
        public override void Dispose()
        {
            this.Close();
        }

        internal StreamInput OpenCachedInputAsStream(string name)
        {
            return new StreamInput(CacheDirectory.OpenInput(name));
        }

        internal StreamOutput CreateCachedOutputAsStream(string name)
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
                        var masterSeg = SegmentInfos.GetCurrentSegmentGeneration(_masterDirectory);
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