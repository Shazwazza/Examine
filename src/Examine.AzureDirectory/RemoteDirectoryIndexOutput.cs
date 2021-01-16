using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Examine.LuceneEngine.Directories;
using Examine.RemoteDirectory;
using Lucene.Net.Store;

namespace Examine.AzureDirectory
{
    /// <summary>
    /// Implements IndexOutput semantics for a write/append only file
    /// </summary>
    public class RemoteDirectoryIndexOutput : IndexOutput
    {
        private readonly AzureLuceneDirectory _azureDirectory;

        //private CloudBlobContainer _blobContainer;
        private readonly string _name;
        private IndexOutput _indexOutput;
        private readonly Mutex _fileMutex;
        private BlobClient _blob;
        private IRemoteDirectory _azureRemoteDirectory;

        public Lucene.Net.Store.Directory CacheDirectory => _azureDirectory.CacheDirectory;

        public RemoteDirectoryIndexOutput(AzureLuceneDirectory azureDirectory, string name)
        {
            //NOTE: _name was null here, is this intended? https://github.com/azure-contrib/AzureDirectory/issues/19 I have changed this to be correct now
            _name = name;
            _azureDirectory = azureDirectory ?? throw new ArgumentNullException(nameof(azureDirectory));
            _azureRemoteDirectory = _azureDirectory.RemoteDirectory;
            _fileMutex = SyncMutexManager.GrabMutex(_azureDirectory, _name);
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
                        _azureDirectory.CompressBlobs, CacheDirectory.FileModified(fileName).ToString());
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
                _blob = null;
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