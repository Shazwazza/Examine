using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Examine.Logging;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Store;

namespace Examine.RemoteDirectory
{
    /// <summary>
    /// Implements IndexInput semantics for a read only blob
    /// </summary>
    public class RemoteDirectoryIndexInput : IndexInput
    {
        private RemoteSyncDirectory _remoteSyncDirectory;
        private readonly string _name;

        private IndexInput _indexInput;
        private readonly Mutex _fileMutex;

        public Lucene.Net.Store.Directory CacheDirectory => _remoteSyncDirectory.CacheDirectory;

        public RemoteDirectoryIndexInput(RemoteSyncDirectory azuredirectory, IRemoteDirectory helper, string name,
            ILoggingService loggingService)
        {
            _name = name;
            _name = _name.Split(new string[] {"%2F"}, StringSplitOptions.RemoveEmptyEntries).Last();
            _remoteSyncDirectory = azuredirectory ?? throw new ArgumentNullException(nameof(azuredirectory));
#if FULLDEBUG
            Trace.WriteLine($"opening {_name} ");
#endif
            _fileMutex = SyncMutexManager.GrabMutex(_remoteSyncDirectory, _name);
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
                    var cachedLength = CacheDirectory.FileLength(fileName);
                    var blobProperties = helper.GetFileProperties(fileName);


                    if (cachedLength != blobProperties.Item1)
                        fFileNeeded = true;
                    else
                    {
                        // cachedLastModifiedUTC was not ouputting with a date (just time) and the time was always off
                        var unixDate = CacheDirectory.FileModified(fileName);
                        var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        var cachedLastModifiedUtc = start.AddMilliseconds(unixDate).ToUniversalTime();

                        if (cachedLastModifiedUtc != blobProperties.Item2)
                        {
                            var timeSpan = blobProperties.Item2.Subtract(cachedLastModifiedUtc);
                            if (timeSpan.TotalSeconds > 1)
                                fFileNeeded = true;
                            else
                            {
#if FULLDEBUG
                                Trace.WriteLine(timeSpan.TotalSeconds);
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
                    helper.SyncFile(CacheDirectory, fileName, azuredirectory.CompressBlobs);

                    // and open it as an input 
                    _indexInput = CacheDirectory.OpenInput(fileName);
                }
                else
                {
#if FULLDEBUG
                    Trace.WriteLine($"Using cached file for {_name}");
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


        public RemoteDirectoryIndexInput(RemoteDirectoryIndexInput cloneInput)
        {
            _name = cloneInput._name;
            _remoteSyncDirectory = cloneInput._remoteSyncDirectory;

            if (string.IsNullOrWhiteSpace(_name)) throw new ArgumentNullException(nameof(cloneInput._name));
            if (_remoteSyncDirectory == null) throw new ArgumentNullException(nameof(cloneInput._remoteSyncDirectory));

            _fileMutex = SyncMutexManager.GrabMutex(cloneInput._remoteSyncDirectory, cloneInput._name);
            _fileMutex.WaitOne();

            try
            {
#if FULLDEBUG
                Trace.WriteLine($"Creating clone for {cloneInput._name}");
#endif
                _indexInput = cloneInput._indexInput.Clone() as IndexInput;
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

        public override byte ReadByte()
        {
            return _indexInput.ReadByte();
        }

        public override void ReadBytes(byte[] b, int offset, int len)
        {
            _indexInput.ReadBytes(b, offset, len);
        }

        public override long FilePointer => _indexInput.FilePointer;

        public override void Seek(long pos)
        {
            _indexInput.Seek(pos);
        }

        protected override void Dispose(bool isDiposing)
        {
            _fileMutex.WaitOne();
            try
            {
#if FULLDEBUG
                Trace.WriteLine($"CLOSED READSTREAM local {_name}");
#endif
                _indexInput.Dispose();
                _indexInput = null;
                _remoteSyncDirectory = null;
                GC.SuppressFinalize(this);
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }

        public override long Length()
        {
            return _indexInput.Length();
        }

        public override object Clone()
        {
            IndexInput clone = null;
            try
            {
                _fileMutex.WaitOne();
                var input = new RemoteDirectoryIndexInput(this);
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