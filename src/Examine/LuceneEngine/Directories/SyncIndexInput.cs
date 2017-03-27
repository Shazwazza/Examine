using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading;
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
    [SecurityCritical]
    internal class SyncIndexInput : IndexInput
    {
        private SyncDirectory _syncDirectory;
        private readonly string _name;

        private IndexInput _indexInput;
        private readonly Mutex _fileMutex;

        public Directory CacheDirectory => _syncDirectory.CacheDirectory;
        public Directory MasterDirectory => _syncDirectory.MasterDirectory;

        /// <summary>
        /// Constructor to create Lucene IndexInput for reading index files
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="name"></param>
        public SyncIndexInput(SyncDirectory directory, string name)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));

            _name = name;
            _syncDirectory = directory;

#if FULLDEBUG
            Trace.WriteLine($"opening {_name} ");
#endif
            _fileMutex = SyncMutexManager.GrabMutex(_syncDirectory, _name);
            _fileMutex.WaitOne();
            try
            {                
                var fileName = _name;

                var fileNeeded = false;
                bool fileExists;

                //Check the cache folder for the file existing, it doesn't exist then the file is needed
                if (!CacheDirectory.FileExists(fileName))
                {
                    fileNeeded = true;
                    fileExists = false;
                }
                else
                {
                    //Hrm, if the file already exists I think we're already in trouble but here's some logic to deal with that anyways.
                    fileExists = true;

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

                    _indexInput = CacheDirectory.OpenInput(fileName);
                }
                else
                {
#if FULLDEBUG
                    Trace.WriteLine($"Using cached file for {_name}");
#endif

                    _indexInput = CacheDirectory.OpenInput(fileName);
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
        public SyncIndexInput(SyncIndexInput cloneInput)
        {
            _name = cloneInput._name;
            _syncDirectory = cloneInput._syncDirectory;

            if (string.IsNullOrWhiteSpace(_name)) throw new ArgumentNullException(nameof(cloneInput._name));
            if (_syncDirectory == null) throw new ArgumentNullException(nameof(cloneInput._syncDirectory));

            _fileMutex = SyncMutexManager.GrabMutex(cloneInput._syncDirectory, cloneInput._name);
            _fileMutex.WaitOne();

            try
            {
#if FULLDEBUG
                Trace.WriteLine($"Creating clone for {cloneInput._name}");
#endif          
                _indexInput = (IndexInput)cloneInput._indexInput.Clone();
            }
            catch (Exception)
            {
                // sometimes we get access denied on the 2nd stream...but not always. I haven't tracked it down yet
                // but this covers our tail until I do
                Trace.TraceError($"Dagnabbit, falling back to memory clone for {cloneInput._name}");
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
                //this will be a file not found

                //TODO: It has been seen that OpenInput on the master can throw an exception due to a lucene file not found - which is very odd
                // we need to check if the master is being written frist before the sync dir. And if the file does not exist in the master, 
                // or the sync dir, then something has gone wrong, that shouldn't happen and we'll need to deal with that differently
                // because the index will be in a state where it's just not readable.
                //Hrmmm what to do?
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
                    cacheOutput?.Close();
                    masterInput?.Close();
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
                Trace.WriteLine("SyncIndexInput file timespan offset: " + timeSpan.TotalSeconds);
#endif
                // file not needed
            }

            return false;
        }

        [SecurityCritical]
        public override byte ReadByte()
        {
            return _indexInput.ReadByte();
        }

        [SecurityCritical]
        public override void ReadBytes(byte[] b, int offset, int len)
        {
            _indexInput.ReadBytes(b, offset, len);
        }

        [SecurityCritical]
        public override long GetFilePointer()
        {
            return _indexInput.GetFilePointer();
        }

        [SecurityCritical]
        public override void Seek(long pos)
        {
            _indexInput.Seek(pos);
        }

        [SecurityCritical]
        public override void Close()
        {
            _fileMutex.WaitOne();
            try
            {
#if FULLDEBUG
                Trace.WriteLine($"CLOSED READSTREAM local {_name}");
#endif
                _indexInput.Close();
                _indexInput = null;
                _syncDirectory = null;
                GC.SuppressFinalize(this);
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }

        [SecurityCritical]
        public override long Length()
        {
            return _indexInput.Length();
        }

        [SecuritySafeCritical]
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

    }
}