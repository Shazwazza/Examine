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
    /// Implements "Copy on write" semantics for Lucene IndexInput
    /// </summary>
    [SecurityCritical]
    internal class SyncIndexOutput : IndexOutput
    {
        private readonly SyncDirectory _syncDirectory;
        private readonly string _name;
        private IndexOutput _indexOutput;
        private readonly Mutex _fileMutex;

        public Directory CacheDirectory => _syncDirectory.CacheDirectory;

        public Directory MasterDirectory => _syncDirectory.MasterDirectory;

        public SyncIndexOutput(SyncDirectory syncDirectory, string name)
        {
            if (syncDirectory == null) throw new ArgumentNullException(nameof(syncDirectory));

            //NOTE: _name was null here https://github.com/azure-contrib/AzureDirectory/issues/19
            // I have changed this to be correct now
            _name = name;
            _syncDirectory = syncDirectory;
            _fileMutex = SyncMutexManager.GrabMutex(_syncDirectory, _name);
            _fileMutex.WaitOne();
            try
            {
                // create the local cache one we will operate against...
                _indexOutput = CacheDirectory.CreateOutput(_name);
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }

        [SecurityCritical]
        public override void Flush()
        {
            _indexOutput.Flush();
        }

        /// <summary>
        /// When the IndexOutput is closed we ensure that the file is flushed and written locally and also persisted to master storage
        /// </summary>
        [SecurityCritical]
        public override void Close()
        {
            _fileMutex.WaitOne();
            try
            {
                var fileName = _name;

                //make sure it's all written out
                //we are only checking for null here in case Close is called multiple times
                if (_indexOutput != null)
                {
                    _indexOutput.Flush();
                    _indexOutput.Close();

                    IndexInput cacheInput = null;
                    try
                    {
                        cacheInput = CacheDirectory.OpenInput(fileName);
                    }
                    catch (IOException e)
                    {
                        //This would occur if the file doesn't exist! we previously threw when that happens so we'll keep
                        //doing that for now but this is quicker than first checking if it exists and then opening it.
                        throw;
                    }

                    if (cacheInput != null)
                    {                        
                        IndexOutput masterOutput = null;
                        try
                        {
                            masterOutput = MasterDirectory.CreateOutput(fileName);
                            cacheInput.CopyTo(masterOutput, fileName);
                        }
                        finally
                        {
                            masterOutput?.Close();
                            cacheInput?.Close();
                        }
                    }

#if FULLDEBUG
                    Trace.WriteLine($"CLOSED WRITESTREAM {_name}");
#endif

                    // clean up
                    _indexOutput = null;
                }

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
            return _indexOutput.Length();
        }

        [SecurityCritical]
        public override void WriteByte(byte b)
        {
            _indexOutput.WriteByte(b);
        }

        [SecurityCritical]
        public override void WriteBytes(byte[] b, int length)
        {
            _indexOutput.WriteBytes(b, length);
        }

        [SecurityCritical]
        public override void WriteBytes(byte[] b, int offset, int length)
        {
            _indexOutput.WriteBytes(b, offset, length);
        }

        [SecurityCritical]
        public override long GetFilePointer()
        {
            return _indexOutput.GetFilePointer();
        }

        [SecurityCritical]
        public override void Seek(long pos)
        {
            _indexOutput.Seek(pos);
        }
    }
}