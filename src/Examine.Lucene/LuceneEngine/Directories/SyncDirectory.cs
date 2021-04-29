using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using Examine.Logging;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Directories
{
    /// <summary>
    /// A custom Lucene directory that allows for a slow master file directory and a fast cache directory
    /// </summary>
    /// <remarks>
    /// This directory is needed in circumstances where the website is hosted on a remote file share or the index exists on a slower
    /// file server. This will allow for the master index copy to be copied to a local fast cache directory which all reads will be 
    /// done through. All writes will then be to both of these directories.
    /// This can be called Copy-on-read and Copy-on-write.
    /// 
    /// Shans notes: 
    /// After some research into an error we were getting it turns out there's a java implementation similar to this called jackrabbit.
    /// They've approached this slightly differently but the end result is very similar. Here's the source code for the interesting parts:
    /// 
    /// CopyOnRead directory:  http://svn.apache.org/viewvc/jackrabbit/oak/branches/1.6/oak-lucene/src/main/java/org/apache/jackrabbit/oak/plugins/index/lucene/directory/CopyOnReadDirectory.java?view=markup
    /// CopyOnWrite directory: http://svn.apache.org/viewvc/jackrabbit/oak/branches/1.6/oak-lucene/src/main/java/org/apache/jackrabbit/oak/plugins/index/lucene/directory/CopyOnWriteDirectory.java?view=markup
    /// 
    /// Whats quite interesting is that they keep an in memory file map of the slow directory which would improve performance if we did that as well since
    /// there would be less IO especially for things like ListAll, FileExists
    /// 
    /// </remarks>
    
    public class SyncDirectory : Lucene.Net.Store.Directory
    {
        private readonly Lucene.Net.Store.Directory _masterDirectory;
        private readonly Lucene.Net.Store.Directory _cacheDirectory;
        private readonly ILoggingService _logging;
        private readonly MultiIndexLockFactory _lockFactory;
        private volatile bool _dirty = true;
        private bool _inSync = false;
        private readonly object _locker = new object();

        internal static readonly HashSet<string> RemoteOnlyFiles = new HashSet<string> {"segments.gen"};


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
            _logging = new TraceLoggingService();
        }
        /// <summary>
        /// Create a SyncDirectory
        /// </summary>
        /// <param name="masterDirectory"></param>
        /// <param name="cacheDirectory">local Directory object to use for local cache</param>
        /// <param name="loggingService">logging service use for tracking errors, info and warnings</param>
        public SyncDirectory(
            Lucene.Net.Store.Directory masterDirectory,
            Lucene.Net.Store.Directory cacheDirectory, ILoggingService loggingService)
        {
            if (masterDirectory == null) throw new ArgumentNullException(nameof(masterDirectory));
            if (cacheDirectory == null) throw new ArgumentNullException(nameof(cacheDirectory));
            _masterDirectory = masterDirectory;
            _cacheDirectory = cacheDirectory;
            _lockFactory = new MultiIndexLockFactory(_masterDirectory, _cacheDirectory);
            _logging = loggingService;
        }
        public void ClearCache()
        {
            foreach (string file in _cacheDirectory.ListAll())
            {
                _cacheDirectory.DeleteFile(file);
            }
        }
        
        public Lucene.Net.Store.Directory CacheDirectory => _cacheDirectory;

        public Lucene.Net.Store.Directory MasterDirectory => _masterDirectory;
        
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
        
        public override string[] ListAll()
        {
            CheckDirty();

            return _inSync ? _cacheDirectory.ListAll() : _masterDirectory.ListAll();
        }

        /// <summary>Returns true if a file with the given name exists. </summary>
        
        [Obsolete("this method will be removed in 5.0 of Lucene")]
        public override bool FileExists(string name)
        {
            CheckDirty();

            if (_inSync)
            {
                //Any exception that is thrown would be based on the ctor of a FileInfo which would indicate security issues or similar
                //see exceptions: https://msdn.microsoft.com/en-us/library/system.io.fileinfo.fileinfo(v=vs.110).aspx
                //Lucene uses a new FileInfo(...).Exists for this check which itself doesn't throw exceptions
                //we used to catch this but it seems counter intuitive since the normal FSDirectory already catches and it seems that
                //if we catch and return null we're covering up an underlying issue.

                var cacheExists = _cacheDirectory.FileExists(name);

                //revert to checking the master - what implications would this have?

                //TODO: If the master does in fact have the file, we should sync it to the cache dir

                return cacheExists || _masterDirectory.FileExists(name);
            }

            //not in sync so return from the master
            return _masterDirectory.FileExists(name);
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
        
        /// <summary>Returns the length of a file in the directory. </summary>
        public override long FileLength(string name)
        {
            CheckDirty();

            return _inSync ? _cacheDirectory.FileLength(name) : _masterDirectory.FileLength(name);
        }

        public override IndexOutput CreateOutput(string name, IOContext context)
        {
            SetDirty();

            //This is what enables "Copy on write" semantics
            return new SyncIndexOutput(this, name, _logging);
        }

        public override void Sync(ICollection<string> names)
        {
            this.EnsureOpen();
        }

        



        /// <summary>Returns a stream reading an existing file. </summary>
        
        public override IndexInput OpenInput(string name, IOContext context)
        {
            //There's a project called Jackrabbit which performs a copy on read/copy on write semantics as well and it appears
            //that they also experienced the file not found issue. I noticed that their implementation only ever reads the segments.gen
            //file from the remote file system. 

            //The Lucene docs reveal a bit more info - since the segments.gen file is not 'write once' we'd have to deal with that accordingly:

            //"As of 2.1, there is also a file segments.gen. This file contains the current generation (the _N in segments_N) of the index. 
            //This is used only as a fallback in case the current generation cannot be accurately determined by directory listing alone 
            //(as is the case for some NFS clients with time-based directory cache expiraation). 
            //This file simply contains an Int32 version header (SegmentInfos.FORMAT_LOCKLESS = -2), followed by the 
            //generation recorded as Int64, written twice."

            //"As of version 2.1 (lock-less commits), file names are never re-used (there is one exception, "segments.gen", see below). 
            //That is, when any file is saved to the Directory it is given a never before used filename. This is achieved using a simple 
            //generations approach. For example, the first segments file is segments_1, then segments_2, etc. 
            //The generation is a sequential long integer represented in alpha-numeric (base 36) form."

            if (RemoteOnlyFiles.Contains(name))
            {
                return _masterDirectory.OpenInput(name,context);
            }

            try
            {
                return new SyncIndexInput(this, name, _logging);
            }
            catch (Exception err)
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

        public override string GetLockID()
        {
            return string.Concat(_masterDirectory.GetLockID(), _cacheDirectory.GetLockID());
        }
        
        protected override void Dispose(bool disposing)
        {
            _masterDirectory.Dispose();
            _cacheDirectory.Dispose();
        }

        public override void SetLockFactory(LockFactory lockFactory)
        {
            _lockFactory = (MultiIndexLockFactory) lockFactory;
        }

        //TODO: This isn't used
        internal StreamInput OpenCachedInputAsStream(string name)
        {
            return new StreamInput(CacheDirectory.OpenInput(name, new IOContext()));
        }

        //TODO: This isn't used
        internal StreamOutput CreateCachedOutputAsStream(string name)
        {
            return new StreamOutput(CacheDirectory.CreateOutput(name,new IOContext()));
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
                        var masterSeg = SegmentInfos.GetLastCommitGeneration(_masterDirectory);
                        var localSeg = SegmentInfos.GetLastCommitGeneration(_cacheDirectory);
                        _inSync = masterSeg == localSeg && masterSeg != -1;
                        _dirty = false;
                    }
                }
            }
        }

        private void SetDirty()
        {
            lock (_locker)
            {
                _dirty = true;
            }
        }
    }
}