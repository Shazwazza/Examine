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
    
    internal class SyncIndexOutput : IndexOutput
    {
        private readonly SyncDirectory _syncDirectory;
        private readonly string _name;
        private IndexOutput _cacheDirIndexOutput;
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
                _cacheDirIndexOutput = CacheDirectory.CreateOutput(_name);
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }

        
        public override void Flush()
        {
            _cacheDirIndexOutput.Flush();
        }

        public override void WriteByte(byte b)
        {
            _cacheDirIndexOutput.WriteByte(b);
        }

        
        public override void WriteBytes(byte[] b, int length)
        {
            _cacheDirIndexOutput.WriteBytes(b, length);
        }

        
        public override void WriteBytes(byte[] b, int offset, int length)
        {
            _cacheDirIndexOutput.WriteBytes(b, offset, length);
        }

        /// <summary>
        /// When the IndexOutput is closed we ensure that the file is flushed and written locally and also persisted to master storage
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            _fileMutex.WaitOne();

            try
            {
                var fileName = _name;

                //make sure it's all written out
                //we are only checking for null here in case Close is called multiple times
                if (_cacheDirIndexOutput != null)
                {
                    _cacheDirIndexOutput.Flush();
                    _cacheDirIndexOutput.Dispose();

                    IndexInput cacheInput = null;
                    try
                    {
                        cacheInput = CacheDirectory.OpenInput(fileName);
                    }
                    catch (IOException)
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
                            masterOutput?.Dispose();
                            cacheInput?.Dispose();
                        }
                    }

#if FULLDEBUG
                    Trace.WriteLine($"CLOSED WRITESTREAM {_name}");
#endif

                    // clean up
                    _cacheDirIndexOutput = null;
                }

                GC.SuppressFinalize(this);
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }

        public override void Seek(long pos)
        {
            _cacheDirIndexOutput.Seek(pos);
        }

        public override long FilePointer => _cacheDirIndexOutput.FilePointer;

        public override long Length => _cacheDirIndexOutput.Length;
    }
}