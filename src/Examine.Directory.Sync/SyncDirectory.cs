using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Lucene.Net.Store;

namespace Examine.Directory.Sync
{
    public class SyncDirectory : Lucene.Net.Store.Directory
    {
        private readonly Lucene.Net.Store.Directory _masterDirectory;
        private readonly Lucene.Net.Store.Directory _cacheDirectory;
        private readonly MultiIndexLockFactory _lockFactory;


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
        public override String[] List()
        {
            //proxy to the underlying non obsolete ListAll
            return _masterDirectory.ListAll();
        }

        /// <summary>Returns an array of strings, one for each file in the
        /// directory.  Unlike <see cref="M:Lucene.Net.Store.Directory.List" /> this method does no
        /// filtering of the contents in a directory, and it will
        /// never return null (throws <see cref="NoSuchDirectoryException"/> instead).
        /// </summary>
        /// <exception cref="NoSuchDirectoryException">
        /// This will throw a <see cref="NoSuchDirectoryException"/> which is expected by lucene when the directory doesn't exist yet
        /// </exception>
        public override string[] ListAll()
        {
            //proxy to the underlying non obsolete ListAll
            return _masterDirectory.ListAll();
        }

        /// <summary>Returns true if a file with the given name exists. </summary>
        public override bool FileExists(String name)
        {
            // this always comes from the server
            try
            {
                return _masterDirectory.FileExists(name);
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
                return _masterDirectory.FileModified(name);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>Set the modified time of an existing file to now. </summary>
        public override void TouchFile(System.String name)
        {
            _cacheDirectory.TouchFile(name);
        }

        /// <summary>Removes an existing file in the directory. </summary>
        public override void DeleteFile(System.String name)
        {
            //We're going to try to remove this from the cache directory first,
            // when _cacheDirectory.DeleteFile(name); is called it magically somehow calls in to our
            // own FileExists method, I cannot figure out why that happens but if we remove the file
            // from the master folder first, then the FileExists will return false because that queries
            // the master folder and if it returns false then it will not be removed from the cache folder.
            try
            {
                if (_cacheDirectory.FileExists(name))
                {
                    _cacheDirectory.DeleteFile(name);
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
                _masterDirectory.RenameFile(from, to);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not rename file on master index; " + ex);
            }

            try
            {
                // we delete and force a redownload, since we can't do this in an atomic way
                if (_cacheDirectory.FileExists(from))
                    _cacheDirectory.RenameFile(from, to);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not rename file on local index; " + ex);
            }
        }

        /// <summary>Returns the length of a file in the directory. </summary>
        public override long FileLength(String name)
        {
            return _masterDirectory.FileLength(name);
        }

        /// <summary>Creates a new, empty file in the directory with the given name.
        /// Returns a stream writing this file. 
        /// </summary>
        public override IndexOutput CreateOutput(System.String name)
        {
            return new SyncIndexOutput(this, name);
        }

        /// <summary>Returns a stream reading an existing file. </summary>
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
        public override Lock MakeLock(System.String name)
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

        /// <summary>
        /// Return a string identifier that uniquely differentiates
        ///             this Directory instance from other Directory instances.
        ///             This ID should be the same if two Directory instances
        ///             (even in different JVMs and/or on different machines)
        ///             are considered "the same index".  This is how locking
        ///             "scopes" to the right index.
        /// 
        /// </summary>
        public override string GetLockID()
        {
            return string.Concat(_masterDirectory.GetLockID(), _cacheDirectory.GetLockID());
        }

        /// <summary>Closes the store. </summary>
        public override void Close()
        {
            _masterDirectory.Close();
            _cacheDirectory.Close();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
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

    }
}