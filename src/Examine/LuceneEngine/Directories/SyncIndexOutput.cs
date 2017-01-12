using System;
using System.Diagnostics;
using System.Security;
using System.Threading;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Directories
{
    /// <summary>
    /// Implements IndexOutput semantics for a write/append only file
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

            //TODO: _name was null here, is this intended? https://github.com/azure-contrib/AzureDirectory/issues/19
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

        [SecurityCritical]
        public override void Close()
        {
            _fileMutex.WaitOne();
            try
            {
                string fileName = _name;

                // make sure it's all written out
                if (_indexOutput != null)
                {
                    _indexOutput.Flush();
                    _indexOutput.Close();
                }

                if (CacheDirectory.FileExists(fileName))
                {
                    //open stream to read cache file
                    using (var cacheStream = new StreamInput(CacheDirectory.OpenInput(fileName)))
                    // push the blobStream up to the master
                    using (var masterStream = new StreamOutput(MasterDirectory.CreateOutput(fileName)))
                    {
                        cacheStream.CopyTo(masterStream);

                        masterStream.Flush();
                        Trace.WriteLine($"PUT {cacheStream.Length} bytes to {_name} in cloud");
                    }

                    //sync the last file write times - at least get them close.
                    //TODO: The alternative would be to force both directory instances to be FSDirectory, 
                    // or try casting the master directory to FSDirectory to get the raw FileInfo and manually
                    // set the lastmodified time - this should work though
                    MasterDirectory.TouchFile(fileName);
                    CacheDirectory.TouchFile(fileName);

#if FULLDEBUG
                      Debug.WriteLine($"CLOSED WRITESTREAM {_name}");
#endif    
                }

                // clean up
                _indexOutput = null;
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