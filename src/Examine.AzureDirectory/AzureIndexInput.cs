using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Azure.Storage.Blobs;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Store;

namespace Examine.AzureDirectory
{
    /// <summary>
    /// Implements IndexInput semantics for a read only blob
    /// </summary>
    public class AzureIndexInput : IndexInput
    {
        private AzureLuceneDirectory _azureDirectory;
        private BlobClient _blob;
        private readonly string _name;

        private IndexInput _indexInput;
        private readonly Mutex _fileMutex;

        public Lucene.Net.Store.Directory CacheDirectory => _azureDirectory.CacheDirectory;

        public AzureIndexInput(AzureLuceneDirectory azuredirectory, BlobClient blob)
        {
            _name = blob.Uri.Segments[blob.Uri.Segments.Length - 1];
            _azureDirectory = azuredirectory ?? throw new ArgumentNullException(nameof(azuredirectory));
#if FULLDEBUG
            Trace.WriteLine($"opening {_name} ");
#endif
            _fileMutex = SyncMutexManager.GrabMutex(_azureDirectory, _name);
            _fileMutex.WaitOne();
            try
            {                
                _blob = blob;

                var fileName = _name;

                var fFileNeeded = false;
                if (!CacheDirectory.FileExists(fileName))
                {
                    fFileNeeded = true;
                }
                else
                {
                    var cachedLength = CacheDirectory.FileLength(fileName);

                    var blobPropertiesResponse = blob.GetProperties();
                    var blobProperties = blobPropertiesResponse.Value;
                    var hasMetadataValue = blobProperties.Metadata.TryGetValue("CachedLength", out var blobLengthMetadata); 
                    var blobLength = blobProperties.ContentLength;
                    if (hasMetadataValue) long.TryParse(blobLengthMetadata, out blobLength);

                    var blobLastModifiedUtc = blobProperties.LastModified.UtcDateTime;
                    if (blobProperties.Metadata.TryGetValue("CachedLastModified", out var blobLastModifiedMetadata))
                    {
                        if (long.TryParse(blobLastModifiedMetadata, out var longLastModified))
                            blobLastModifiedUtc = new DateTime(longLastModified).ToUniversalTime();
                    }
                    
                    if (cachedLength != blobLength)
                        fFileNeeded = true;
                    else
                    {

                        // cachedLastModifiedUTC was not ouputting with a date (just time) and the time was always off
                        var unixDate = CacheDirectory.FileModified(fileName);
                        var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        var cachedLastModifiedUtc = start.AddMilliseconds(unixDate).ToUniversalTime();
                        
                        if (cachedLastModifiedUtc != blobLastModifiedUtc)
                        {
                            var timeSpan = blobLastModifiedUtc.Subtract(cachedLastModifiedUtc);
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
                    if (_azureDirectory.ShouldCompressFile(_name))
                    {
                        InflateStream(fileName);
                    }
                    else
                    {
                        using (var fileStream = new StreamOutput(CacheDirectory.CreateOutput(fileName)))
                        {
                            // get the blob
                            _blob.DownloadTo(fileStream);

                            fileStream.Flush();
#if FULLDEBUG
                            Trace.WriteLine($"GET {_name} RETREIVED {fileStream.Length} bytes");
#endif
                        }
                    }

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

        private void InflateStream(string fileName)
        {
            // then we will get it fresh into local deflatedName 
            // StreamOutput deflatedStream = new StreamOutput(CacheDirectory.CreateOutput(deflatedName));
            using (var deflatedStream = new MemoryStream())
            {
                // get the deflated blob
                _blob.DownloadTo(deflatedStream);

#if FULLDEBUG
                Trace.WriteLine($"GET {_name} RETREIVED {deflatedStream.Length} bytes");
#endif 

                // seek back to begininng
                deflatedStream.Seek(0, SeekOrigin.Begin);

                // open output file for uncompressed contents
                using (var fileStream = new StreamOutput(CacheDirectory.CreateOutput(fileName)))
                using (var decompressor = new DeflateStream(deflatedStream, CompressionMode.Decompress))
                {
                    var bytes = new byte[65535];
                    var nRead = 0;
                    do
                    {
                        nRead = decompressor.Read(bytes, 0, 65535);
                        if (nRead > 0)
                            fileStream.Write(bytes, 0, nRead);
                    } while (nRead == 65535);
                }
            }
        }

        public AzureIndexInput(AzureIndexInput cloneInput)
        {
            _name = cloneInput._name;
            _azureDirectory = cloneInput._azureDirectory;
            _blob = cloneInput._blob;

            if (string.IsNullOrWhiteSpace(_name)) throw new ArgumentNullException(nameof(cloneInput._name));
            if (_azureDirectory == null) throw new ArgumentNullException(nameof(cloneInput._azureDirectory));
            if (_blob == null) throw new ArgumentNullException(nameof(cloneInput._blob));

            _fileMutex = SyncMutexManager.GrabMutex(cloneInput._azureDirectory, cloneInput._name);
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
                _azureDirectory = null;
                _blob = null;
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
                var input = new AzureIndexInput(this);
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
