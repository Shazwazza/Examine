using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading;
using Examine.Logging;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.LuceneEngine.Directories
{
    /// <summary>
    /// Implements "Copy on read" semantics for Lucene IndexInput
    /// </summary>
    /// <remarks>
    /// When a file is requested for reading, we need to ensure that file exists locally in the cache folder if it's not 
    /// already there. This is synced from the main master directory.
    /// </remarks>
    
    internal class SyncIndexInput : IndexInput
    {
        private SyncDirectory _syncDirectory;
        private readonly string _name;
        private readonly ILoggingService _loggingService;

        private IndexInput _cacheDirIndexInput;
        private readonly Mutex _fileMutex;

        public Directory CacheDirectory => _syncDirectory.CacheDirectory;
        public Directory MasterDirectory => _syncDirectory.MasterDirectory;

        /// <summary>
        /// Constructor to create Lucene IndexInput for reading index files
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="name"></param>
        /// <remarks>
        /// This will not work for the segments.gen file because it doesn't compare to master and segments.gen is not write-once!
        /// Therefore do not use this class from a Directory instance for that file, see SyncDirectory.OpenInput
        /// </remarks>
        public SyncIndexInput(SyncDirectory directory, string name) : this(directory, name, new TraceLoggingService())
        {
        }
        /// <summary>
        /// Constructor to create Lucene IndexInput for reading index files
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="name"></param>
        /// <param name="loggingService"></param>
        /// <remarks>
        /// This will not work for the segments.gen file because it doesn't compare to master and segments.gen is not write-once!
        /// Therefore do not use this class from a Directory instance for that file, see SyncDirectory.OpenInput
        /// </remarks>
        public SyncIndexInput(SyncDirectory directory, string name, ILoggingService loggingService)
        {
            
            if (directory == null) throw new ArgumentNullException(nameof(directory));

            _name = name;
            _loggingService = loggingService;
            _syncDirectory = directory;

#if FULLDEBUG
            loggingService.Log(new LogEntry(LogLevel.Info, null,$"opening {_name} " ));
#endif
            _fileMutex = SyncMutexManager.GrabMutex(_syncDirectory, _name);
            _fileMutex.WaitOne();
            try
            {                
                var fileName = _name;

                var fileNeeded = false;

                //Check the cache folder for the file existing, it doesn't exist then the file is needed
                if (!CacheDirectory.FileExists(fileName))
                {
                    fileNeeded = true;
                }
                else
                {

                    //Normally we'd compare the file attributes to see if we need to override it but Lucene has write-once semantics so
                    //this is unecessary. If the file already exists locally then we will have to assume it is the correct file!

                    //fileNeeded = CompareExistingFileAttributes(fileName);
                    fileNeeded = false;
                    
                }

                // if the file does not exist
                // or if it exists and it is older then the lastmodified time in the blobproperties (which always comes from the blob storage)
                if (fileNeeded)
                {
                    SyncLocally(fileName);
                    _cacheDirIndexInput = CacheDirectory.OpenInput(fileName);
                }
                else
                {
#if FULLDEBUG
                    loggingService.Log(new LogEntry(LogLevel.Info, null,$"Using cached file for {_name}"));
#endif

                    _cacheDirIndexInput = CacheDirectory.OpenInput(fileName);
                }
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Constructor used for cloning
        /// </summary>
        /// <param name="cloneInput"></param>
        public SyncIndexInput(SyncIndexInput cloneInput) : this(cloneInput, new TraceLoggingService())
        {
            
        }
        public SyncIndexInput(SyncIndexInput cloneInput, ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _name = cloneInput._name;
            _syncDirectory = cloneInput._syncDirectory;

            if (string.IsNullOrWhiteSpace(_name)) throw new ArgumentNullException(nameof(cloneInput._name));
            if (_syncDirectory == null) throw new ArgumentNullException(nameof(cloneInput._syncDirectory));

            _fileMutex = SyncMutexManager.GrabMutex(cloneInput._syncDirectory, cloneInput._name);
            _fileMutex.WaitOne();

            try
            {
#if FULLDEBUG
                loggingService.Log(new LogEntry(LogLevel.Info, null,$"Creating clone for {cloneInput._name}"));

#endif          
                _cacheDirIndexInput = (IndexInput)cloneInput._cacheDirIndexInput.Clone();
            }
            catch (Exception e)
            {
                // sometimes we get access denied on the 2nd stream...but not always. I haven't tracked it down yet
                // but this covers our tail until I do
                loggingService.Log(new LogEntry(LogLevel.Error, e,$"Dagnabbit, falling back to memory clone for {cloneInput._name}"));
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// This will sync the requested file from master storage to the local fast cache storage
        /// </summary>
        /// <param name="fileName"></param>
        private void SyncLocally(string fileName)
        {
            //get the master file stream

            IndexInput masterInput = null;
            try
            {
                masterInput = MasterDirectory.OpenInput(fileName);                
            }
            catch (IOException ex)
            {
                //this will be a file not found (FileNotFoundException)

                //TODO: It has been seen that OpenInput on the master can throw an exception due to a lucene file not found - which is very odd
                // we need to check if the master is being written first before the sync dir. And if the file does not exist in the master, 
                // or the sync dir, then something has gone wrong, that shouldn't happen and we'll need to deal with that differently
                // because the index will be in a state where it's just not readable.
                //Hrmmm what to do?  There's actually nothing that can be done :/ if we return false here then the instance of this item would be null
                //which will then cause exceptions further on and take down the app pool anyways. I've looked through the Lucene source and there 
                //is no safety net to check against this situation, it just happily throws exceptions on a background thread.
                _loggingService.Log(new LogEntry(LogLevel.Error, ex,$"File not found"));

                throw ex;
            }

            if (masterInput != null)
            {
                IndexOutput cacheOutput = null;
                try
                {
                    cacheOutput = CacheDirectory.CreateOutput(fileName);
                    masterInput.CopyTo(cacheOutput, fileName);

                }
                finally
                {
                    cacheOutput?.Dispose();
                    masterInput?.Dispose();
                }
            }
            
        }

        /// <summary>
        /// This will check if the same file between master and cache dirs is the same
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <remarks>
        /// This logic shouldn't need to execute because all of Lucene files are 'write onces' except for the segments.gen file,
        /// therefore we should never have to compare if the local file is different from the master.
        /// I'm leaving this method here in case we need to look back to it but it's not necessary.
        /// </remarks>
        private bool CompareExistingFileAttributes(string fileName)
        {
            //The file already exists so compare the lengths of the files from the master vs cache
            long cachedLength = CacheDirectory.FileLength(fileName);
            long masterLength = MasterDirectory.FileLength(fileName);

            //if the lengths are not the same, the the file is needed (would need to be deleted/updated)
            if (cachedLength != masterLength)
                return true;


            //The file already exists and the file lengths are the same so compare the modified dates of the files from the master vs cache

            long cacheDate = CacheDirectory.FileModified(fileName);
            long masterDate = MasterDirectory.FileModified(fileName);

            //we need to compare to the second instead of by ticks because when we call 
            //TouchFile in SyncIndexOutput this won't set the files to be identical
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var cachedLastModifiedUTC = start.AddMilliseconds(cacheDate).ToUniversalTime();
            var masterLastModifiedUTC = start.AddMilliseconds(masterDate).ToUniversalTime();

            if (cachedLastModifiedUTC != masterLastModifiedUTC)
            {
                var timeSpan = masterLastModifiedUTC.Subtract(cachedLastModifiedUTC);

                //NOTE: This heavily depends on TouchFile in SyncIndexOutput which sets both the 
                //master and slave files to be 'Now', in theory this operation shouldn't
                //make the file times any bigger than 1 second

                //NOTE: TouchFile isn't actually used by Lucene, BUT it is used by us in the SyncIndexOutput class

                if (timeSpan.TotalSeconds > 2)
                    return true;

#if FULLDEBUG
                _loggingService.Log(new LogEntry(LogLevel.Info, null,$"SyncIndexInput file timespan offset: " + timeSpan.TotalSeconds));
#endif
                // file not needed
            }

            return false;
        }

        
        public override byte ReadByte()
        {
            return _cacheDirIndexInput.ReadByte();
        }

        
        public override void ReadBytes(byte[] b, int offset, int len)
        {
            _cacheDirIndexInput.ReadBytes(b, offset, len);
        }
        
        protected override void Dispose(bool disposing)
        {
            _fileMutex.WaitOne();
            try
            {
#if FULLDEBUG
                _loggingService.Log(new LogEntry(LogLevel.Info, null,$"CLOSED READSTREAM local {_name}"));
#endif
                _cacheDirIndexInput.Dispose();
                _cacheDirIndexInput = null;
                _syncDirectory = null;
                GC.SuppressFinalize(this);
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }

        public override void Seek(long pos)
        {
            _cacheDirIndexInput.Seek(pos);
        }

        public override long Length()
        {
            return _cacheDirIndexInput.Length();
        }

        
        public override object Clone()
        {
            IndexInput clone = null;
            try
            {
                _fileMutex.WaitOne();
                var input = new SyncIndexInput(this);
                clone = input;
            }
            catch (Exception err)
            {
                Trace.TraceError(err.ToString());
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
            Debug.Assert(clone != null);
            return clone;
        }

        public override long FilePointer => _cacheDirIndexInput.FilePointer;
    }
}