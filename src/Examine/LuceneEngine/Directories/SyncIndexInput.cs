using System;
using System.Diagnostics;
using System.Security;
using System.Threading;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Directories
{
    /// <summary>
    /// Implements IndexInput semantics for a read only blob
    /// </summary>
    [SecurityCritical]
    internal class SyncIndexInput : IndexInput
    {
        private SyncDirectory _syncDirectory;
        private readonly string _name;

        private IndexInput _indexInput;
        private readonly Mutex _fileMutex;

        public Directory CacheDirectory { get { return _syncDirectory.CacheDirectory; } }
        public Directory MasterDirectory { get { return _syncDirectory.MasterDirectory; } }
        
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

                var fFileNeeded = false;
                if (!CacheDirectory.FileExists(fileName))
                {
                    fFileNeeded = true;
                }
                else
                {
                    long cachedLength = CacheDirectory.FileLength(fileName);                    
                    long masterLength = MasterDirectory.FileLength(fileName);
                    
                    if (cachedLength != masterLength)
                        fFileNeeded = true;
                    else
                    {
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
                            if (timeSpan.TotalSeconds > 2)
                                fFileNeeded = true;
                            else
                            {
#if FULLDEBUG
                                Debug.WriteLine(timeSpan.TotalSeconds);
#endif
                                // file not needed
                            }
                        }
                    }
                }

                // if the file does not exist
                // or if it exists and it is older then the lastmodified time in the blobproperties (which always comes from the blob storage)
                if (fFileNeeded)
                {
                    //get the master file stream
                    using (var masterStream = new StreamInput(MasterDirectory.OpenInput(fileName)))
                    using (var cacheStream = _syncDirectory.CreateCachedOutputAsStream(fileName))
                    {
                        //copy this to the cached file stream
                        masterStream.CopyTo(cacheStream);

                        cacheStream.Flush();
                        Debug.WriteLine(string.Format("GET {0} RETREIVED {1} bytes", _name, cacheStream.Length));
                    }                    

                    // and open it as an input 
                    _indexInput = CacheDirectory.OpenInput(fileName);
                }
                else
                {
#if FULLDEBUG
                    Debug.WriteLine(String.Format("Using cached file for {0}", _name));
#endif

                    // open the file in read only mode
                    _indexInput = CacheDirectory.OpenInput(fileName);
                }
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }
        
        public SyncIndexInput(SyncIndexInput cloneInput)
        {
            _fileMutex = SyncMutexManager.GrabMutex(cloneInput._syncDirectory, cloneInput._name);
            _fileMutex.WaitOne();

            try
            {
#if FULLDEBUG
                Debug.WriteLine(String.Format("Creating clone for {0}", cloneInput._name));
#endif
                _syncDirectory = cloneInput._syncDirectory;                
                _indexInput = cloneInput._indexInput.Clone() as IndexInput;
            }
            catch (Exception)
            {
                // sometimes we get access denied on the 2nd stream...but not always. I haven't tracked it down yet
                // but this covers our tail until I do
                Trace.Fail(String.Format("Dagnabbit, falling back to memory clone for {0}", cloneInput._name));
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
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
                Trace.WriteLine(String.Format("CLOSED READSTREAM local {0}", _name));
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
        public override System.Object Clone()
        {
            IndexInput clone = null;
            try
            {
                _fileMutex.WaitOne();
                SyncIndexInput input = new SyncIndexInput(this);
                clone = (IndexInput)input;
            }
            catch (System.Exception err)
            {
                Trace.WriteLine(err.ToString());
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