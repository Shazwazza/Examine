using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Lucene.Net.Store;

namespace Examine.Directory.Sync
{
    /// <summary>
    /// Implements IndexOutput semantics for a write/append only file
    /// </summary>
    internal class SyncIndexOutput : IndexOutput
    {
        private readonly SyncDirectory _syncDirectory;
        private readonly string _name;
        private IndexOutput _indexOutput;
        private readonly Mutex _fileMutex;
        public Lucene.Net.Store.Directory CacheDirectory { get { return _syncDirectory.CacheDirectory; } }
        public Lucene.Net.Store.Directory MasterDirectory { get { return _syncDirectory.MasterDirectory; } }

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

        public override void Flush()
        {
            _indexOutput.Flush();
        }

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
                        Trace.WriteLine(string.Format("PUT {1} bytes to {0} in cloud", _name, cacheStream.Length));
                    }

                    //sync the last file write times - at least get them close.
                    //TODO: The alternative would be to force both directory instances to be FSDirectory, 
                    // or try casting the master directory to FSDirectory to get the raw FileInfo and manually
                    // set the lastmodified time - this should work though
                    MasterDirectory.TouchFile(fileName);
                    CacheDirectory.TouchFile(fileName);

#if FULLDEBUG
                      Debug.WriteLine(string.Format("CLOSED WRITESTREAM {0}", _name));
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

        public override long Length()
        {
            return _indexOutput.Length();
        }

        public override void WriteByte(byte b)
        {
            _indexOutput.WriteByte(b);
        }

        public override void WriteBytes(byte[] b, int length)
        {
            _indexOutput.WriteBytes(b, length);
        }

        public override void WriteBytes(byte[] b, int offset, int length)
        {
            _indexOutput.WriteBytes(b, offset, length);
        }

        public override long GetFilePointer()
        {
            return _indexOutput.GetFilePointer();
        }

        public override void Seek(long pos)
        {
            _indexOutput.Seek(pos);
        }
    }
}