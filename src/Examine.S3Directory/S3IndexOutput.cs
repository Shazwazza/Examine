using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Amazon.S3.IO;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.S3Directory
{
    /// <summary>
    /// Implements IndexOutput semantics for a write/append only file
    /// </summary>
    public class S3IndexOutput : IndexOutput
    {
        private readonly S3Directory _s3Directory;
        //private CloudBlobContainer _blobContainer;
        private readonly string _name;
        private IndexOutput _indexOutput;
        private readonly Mutex _fileMutex;
        private S3FileInfo _blob;
        public Directory CacheDirectory => _s3Directory.CacheDirectory;

        public S3IndexOutput(S3Directory s3Directory, S3FileInfo blob, string name)
        {
            //TODO: _name was null here, is this intended? https://github.com/azure-contrib/AzureDirectory/issues/19
            // I have changed this to be correct now
            _name = name;
            _s3Directory = s3Directory ?? throw new ArgumentNullException(nameof(s3Directory));
            _fileMutex = SyncMutexManager.GrabMutex(_s3Directory, _name); 
            _fileMutex.WaitOne();
            try
            {                
                //_blobContainer = _azureDirectory.BlobContainer;
                _blob = blob;
                _name = blob.Name.Split('/')[1];

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
                    Stream blobStream;

                    // optionally put a compressor around the blob stream
                    if (_s3Directory.ShouldCompressFile(_name))
                    {
                        blobStream = CompressStream(fileName, originalLength);
                    }
                    else
                    {
                        blobStream = new StreamInput(CacheDirectory.OpenInput(fileName));
                    }

                    try
                    {
                        // push the blobStream up to the cloud
                        var fileTransferUtility =
                            new TransferUtility(_s3Directory._blobClient);
                           var request = new TransferUtilityUploadRequest();
                           request.Key = _blob.Name;
                           request.BucketName = _s3Directory._containerName;
                           request.InputStream = blobStream;
                           request.Metadata.Add("CachedLength",originalLength.ToString());
                           request.Metadata.Add("CachedLastModified",CacheDirectory.FileModified(fileName).ToString());
                        fileTransferUtility.Upload(request);
                        // set the metadata with the original index file properties

#if FULLDEBUG
                        Trace.WriteLine($"PUT {blobStream.Length} bytes to {_name} in cloud");
#endif
                    }
                    finally
                    {
                        blobStream.Dispose();
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

        private MemoryStream CompressStream(string fileName, long originalLength)
        {
            // unfortunately, deflate stream doesn't allow seek, and we need a seekable stream
            // to pass to the blob storage stuff, so we compress into a memory stream
            MemoryStream compressedStream = new MemoryStream();

            IndexInput indexInput = null;
            try
            {
                indexInput = CacheDirectory.OpenInput(fileName);
                using (var compressor = new DeflateStream(compressedStream, CompressionMode.Compress, true))
                {
                    // compress to compressedOutputStream
                    byte[] bytes = new byte[indexInput.Length()];
                    indexInput.ReadBytes(bytes, 0, bytes.Length);
                    compressor.Write(bytes, 0, bytes.Length);
                }

                // seek back to beginning of comrpessed stream
                compressedStream.Seek(0, SeekOrigin.Begin);
#if FULLDEBUG
                Trace.WriteLine($"COMPRESSED {originalLength} -> {compressedStream.Length} {((float) compressedStream.Length / (float) originalLength) * 100}% to {_name}");
#endif
            }
            catch
            {
                // release the compressed stream resources if an error occurs
                compressedStream.Dispose();
                throw;
            }
            finally
            {
                indexInput?.Close();
            }
            return compressedStream;
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
