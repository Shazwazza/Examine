using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Amazon.S3.IO;
using Amazon.S3.Model;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.S3Directory
{
    /// <summary>
    /// Implements IndexInput semantics for a read S3 object
    /// </summary>
    public class S3IndexInput : IndexInput
    {
        private S3Directory _s3Directory;

        private readonly string _name;

        private IndexInput _indexInput;
        private readonly Mutex _fileMutex;

        public Directory CacheDirectory => _s3Directory.CacheDirectory;

        public S3IndexInput(S3Directory S3directory, S3FileInfo s3Object)
        {
            _name = s3Object.Name.Split('/')[1];
            _s3Directory = S3directory ?? throw new ArgumentNullException(nameof(S3directory));
#if FULLDEBUG
            Trace.WriteLine($"opening {_name} ");
#endif
            _fileMutex = SyncMutexManager.GrabMutex(_s3Directory, _name);
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
                    var s3ObjectLength = s3Object.Length;
                    var s3ObjectLastModifiedUtc = s3Object.LastWriteTimeUtc;
                    GetObjectMetadataRequest request = new GetObjectMetadataRequest();
                    request.Key = s3Object.Name;
                    request.BucketName = S3directory._containerName;
                    GetObjectMetadataResponse response = _s3Directory.S3Client.GetObjectMetadata(request);
                    var CachedLength = response.Metadata["CachedLength"];

                    if (!string.IsNullOrEmpty(CachedLength) && long.TryParse(CachedLength, out var longLastModified))
                        s3ObjectLastModifiedUtc = new DateTime(longLastModified).ToUniversalTime();
                
                    if (cachedLength != s3ObjectLength)
                        fFileNeeded = true;
                    else
                    {

                        // cachedLastModifiedUTC was not ouputting with a date (just time) and the time was always off
                        var unixDate = CacheDirectory.FileModified(fileName);
                        var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        var cachedLastModifiedUtc = start.AddMilliseconds(unixDate).ToUniversalTime();
                        
                        if (cachedLastModifiedUtc != s3ObjectLastModifiedUtc)
                        {
                            var timeSpan = s3ObjectLastModifiedUtc.Subtract(cachedLastModifiedUtc);
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
                // or if it exists and it is older then the lastmodified time in the object properties (which always comes from the s3 storage)
                if (fFileNeeded)
                {
                    if (_s3Directory.ShouldCompressFile(_name))
                    {
                        InflateStream(s3Object,fileName);
                    }
                    else
                    {
                        using (var fileStream = new StreamOutput(CacheDirectory.CreateOutput(fileName)))
                            using(var response = S3directory.S3Client.GetObject(S3directory._containerName,s3Object.Name))
                                using(Stream responseStream =  response.ResponseStream)
                        {
                            
                            // get the s3Object
                            responseStream.CopyTo(fileStream);

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

        private void InflateStream(S3FileInfo s3Object, string name)
        {
            // then we will get it fresh into local deflatedName 
            // StreamOutput deflatedStream = new StreamOutput(CacheDirectory.CreateOutput(deflatedName));
            using (var deflatedStream = new MemoryStream())
            using(var response = _s3Directory.S3Client.GetObject(_s3Directory._containerName,s3Object.Name))
            using(Stream responseStream =  response.ResponseStream)
            {
                            
                // get the s3Object
                responseStream.CopyTo(deflatedStream);

#if FULLDEBUG
                Trace.WriteLine($"GET {_name} RETREIVED {deflatedStream.Length} bytes");
#endif 

                // seek back to begininng
                deflatedStream.Seek(0, SeekOrigin.Begin);

                // open output file for uncompressed contents
                using (var fileStream = new StreamOutput(CacheDirectory.CreateOutput(name)))
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

        public S3IndexInput(S3IndexInput cloneInput)
        {
            _name = cloneInput._name;
            _s3Directory = cloneInput._s3Directory;

            if (string.IsNullOrWhiteSpace(_name)) throw new ArgumentNullException(nameof(cloneInput._name));
            if (_s3Directory == null) throw new ArgumentNullException(nameof(cloneInput._s3Directory));


            _fileMutex = SyncMutexManager.GrabMutex(cloneInput._s3Directory, cloneInput._name);
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
                _s3Directory = null;
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
                var input = new S3IndexInput(this);
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
