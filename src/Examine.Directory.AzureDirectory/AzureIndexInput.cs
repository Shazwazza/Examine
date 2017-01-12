using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Store;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Examine.Directory.AzureDirectory
{
    /// <summary>
    /// Implements IndexInput semantics for a read only blob
    /// </summary>
    public class AzureIndexInput : IndexInput
    {
        private AzureDirectory _azureDirectory;
        private CloudBlobContainer _blobContainer;
        private ICloudBlob _blob;
        private string _name;

        private IndexInput _indexInput;
        private Mutex _fileMutex;

        public Lucene.Net.Store.Directory CacheDirectory { get { return _azureDirectory.CacheDirectory; } }

        public AzureIndexInput(AzureDirectory azuredirectory, ICloudBlob blob)
        {
            if (azuredirectory == null) throw new ArgumentNullException(nameof(azuredirectory));

            _name = blob.Uri.Segments[blob.Uri.Segments.Length - 1];
            _azureDirectory = azuredirectory;
#if FULLDEBUG
            Debug.WriteLine(String.Format("opening {0} ", _name));
#endif
            _fileMutex = SyncMutexManager.GrabMutex(_azureDirectory, _name);
            _fileMutex.WaitOne();
            try
            {                
                _blobContainer = azuredirectory.BlobContainer;
                _blob = blob;

                var fileName = _name;

                var fFileNeeded = false;
                if (!CacheDirectory.FileExists(fileName))
                {
                    fFileNeeded = true;
                }
                else
                {
                    long cachedLength = CacheDirectory.FileLength(fileName);
                    string blobLengthMetadata;
                    bool hasMetadataValue = blob.Metadata.TryGetValue("CachedLength", out blobLengthMetadata); 
                    long blobLength = blob.Properties.Length;
                    if (hasMetadataValue) long.TryParse(blobLengthMetadata, out blobLength);

                    string blobLastModifiedMetadata;
                    long longLastModified = 0;
                    DateTime blobLastModifiedUTC = blob.Properties.LastModified.Value.UtcDateTime;
                    if (blob.Metadata.TryGetValue("CachedLastModified", out blobLastModifiedMetadata)) {
                        if (long.TryParse(blobLastModifiedMetadata, out longLastModified))
                            blobLastModifiedUTC = new DateTime(longLastModified).ToUniversalTime();
                    }
                    
                    if (cachedLength != blobLength)
                        fFileNeeded = true;
                    else
                    {

                        // cachedLastModifiedUTC was not ouputting with a date (just time) and the time was always off
                        long unixDate = CacheDirectory.FileModified(fileName);
                        DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        var cachedLastModifiedUTC = start.AddMilliseconds(unixDate).ToUniversalTime();
                        
                        if (cachedLastModifiedUTC != blobLastModifiedUTC)
                        {
                            var timeSpan = blobLastModifiedUTC.Subtract(cachedLastModifiedUTC);
                            if (timeSpan.TotalSeconds > 1)
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
                    if (_azureDirectory.ShouldCompressFile(_name))
                    {
                        InflateStream(fileName);
                    }
                    else
                    {
                        using (var fileStream = _azureDirectory.CreateCachedOutputAsStream(fileName))
                        {
                            // get the blob
                            _blob.DownloadToStream(fileStream);

                            fileStream.Flush();
                            Debug.WriteLine(string.Format("GET {0} RETREIVED {1} bytes", _name, fileStream.Length));
                        }
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

        private void InflateStream(string fileName)
        {
            // then we will get it fresh into local deflatedName 
            // StreamOutput deflatedStream = new StreamOutput(CacheDirectory.CreateOutput(deflatedName));
            using (var deflatedStream = new MemoryStream())
            {
                // get the deflated blob
                _blob.DownloadToStream(deflatedStream);

                Debug.WriteLine(string.Format("GET {0} RETREIVED {1} bytes", _name, deflatedStream.Length));

                // seek back to begininng
                deflatedStream.Seek(0, SeekOrigin.Begin);

                // open output file for uncompressed contents
                using (var fileStream = _azureDirectory.CreateCachedOutputAsStream(fileName))
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
            _fileMutex = SyncMutexManager.GrabMutex(cloneInput._azureDirectory, cloneInput._name);
            _fileMutex.WaitOne();

            try
            {
#if FULLDEBUG
                Debug.WriteLine(String.Format("Creating clone for {0}", cloneInput._name));
#endif
                _azureDirectory = cloneInput._azureDirectory;
                _blobContainer = cloneInput._blobContainer;
                _blob = cloneInput._blob;
                _indexInput = cloneInput._indexInput.Clone() as IndexInput;
            }
            catch (Exception)
            {
                // sometimes we get access denied on the 2nd stream...but not always. I haven't tracked it down yet
                // but this covers our tail until I do
                Debug.WriteLine(String.Format("Dagnabbit, falling back to memory clone for {0}", cloneInput._name));
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

        public override long GetFilePointer()
        {
            return _indexInput.GetFilePointer();
        }

        public override void Seek(long pos)
        {
            _indexInput.Seek(pos);
        }

        public override void Close()
        {
            _fileMutex.WaitOne();
            try
            {
#if FULLDEBUG
                Debug.WriteLine(String.Format("CLOSED READSTREAM local {0}", _name));
#endif
                _indexInput.Close();
                _indexInput = null;
                _azureDirectory = null;
                _blobContainer = null;
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

        public override System.Object Clone()
        {
            IndexInput clone = null;
            try
            {
                _fileMutex.WaitOne();
                AzureIndexInput input = new AzureIndexInput(this);
                clone = (IndexInput)input;
            }
            catch (System.Exception err)
            {
                Debug.WriteLine(err.ToString());
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
