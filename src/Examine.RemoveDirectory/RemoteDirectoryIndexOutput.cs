using System;
using System.Threading;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Store;

namespace Examine.RemoveDirectory
{
    /// <summary>
    /// Implements IndexOutput semantics for a write/append only file
    /// </summary>
    public class RemoteDirectoryIndexOutput : IndexOutput
    {
        private readonly RemoteSyncDirectory _azureSyncDirectory;

        //private CloudBlobContainer _blobContainer;
        private readonly string _name;
        private IndexOutput _indexOutput;
        private readonly Mutex _fileMutex;
        private IRemoteDirectory _azureRemoteDirectory;

        public Lucene.Net.Store.Directory CacheDirectory => _azureSyncDirectory.CacheDirectory;

        public RemoteDirectoryIndexOutput(RemoteSyncDirectory azureSyncDirectory, string name)
        {
            //NOTE: _name was null here, is this intended? https://github.com/azure-contrib/AzureDirectory/issues/19 I have changed this to be correct now
            _name = name;
            _azureSyncDirectory = azureSyncDirectory ?? throw new ArgumentNullException(nameof(azureSyncDirectory));
            _azureRemoteDirectory = _azureSyncDirectory.RemoteDirectory;
            _fileMutex = SyncMutexManager.GrabMutex(_azureSyncDirectory, _name);
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

        protected override void Dispose(bool isDisposing)
        {
            _fileMutex.WaitOne();
            try
            {
                var fileName = _name;

                long originalLength = 0;

                //this can be null in some odd cases so we need to check
                if (_indexOutput != null)
                {
                    try
                    {
                        // make sure it's all written out
                        _indexOutput.Flush();
                        originalLength = _indexOutput.Length;
                    }
                    finally
                    {
                        _indexOutput.Dispose();
                    }
                }

                if (originalLength > 0)
                {
                    var result = _azureRemoteDirectory.Upload(CacheDirectory.OpenInput(fileName), fileName,
                        originalLength,
                        _azureSyncDirectory.CompressBlobs, CacheDirectory.FileModified(fileName).ToString());
                    // push the blobStream up to the cloud
                    if (result)
                    {
                        throw new Exception("File already exists");
                    }

#if FULLDEBUG
                    Trace.WriteLine($"CLOSED WRITESTREAM {_name}");
#endif
                }

                // clean up
                _indexOutput = null;
                //_blobContainer = null;
                GC.SuppressFinalize(this);
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }


        public override long Length => _indexOutput.Length;

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

        public override long FilePointer => _indexOutput.FilePointer;

        public override void Seek(long pos)
        {
            _indexOutput.Seek(pos);
        }
    }
}