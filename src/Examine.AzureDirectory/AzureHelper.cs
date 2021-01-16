using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Azure.Storage.Blobs;
using Examine.LuceneEngine.Directories;

namespace Examine.AzureDirectory
{
    public class AzureHelper
    {
        /// <summary>. </summary>
        public void SyncFile(Lucene.Net.Store.Directory directory, BlobClient _blob, string fileName,string RootFolder, bool CompressBlobs)
        {
            Trace.WriteLine($"INFO Syncing file {fileName} for {RootFolder}");
            // then we will get it fresh into local deflatedName 
            // StreamOutput deflatedStream = new StreamOutput(CacheDirectory.CreateOutput(deflatedName));
            using (var deflatedStream = new MemoryStream())
            {
                // get the deflated blob
                _blob.DownloadTo(deflatedStream);

#if FULLDEBUG
                Trace.WriteLine($"GET {fileName} RETREIVED {deflatedStream.Length} bytes");
#endif 

                // seek back to begininng
                deflatedStream.Seek(0, SeekOrigin.Begin);

                if (ShouldCompressFile(fileName, CompressBlobs))
                {
                    // open output file for uncompressed contents
                    using (var fileStream = new StreamOutput(directory.CreateOutput(fileName)))
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
                else
                {
                    using (var fileStream = new StreamOutput(directory.CreateOutput(fileName)))
                    {
                        // get the blob
                        _blob.DownloadTo(fileStream);

                        fileStream.Flush();
#if FULLDEBUG
                        Trace.WriteLine($"GET {fileName} RETREIVED {fileStream.Length} bytes");
#endif
                    }
                }

            }
        }
        public BlobContainerClient EnsureContainer(string _storageAccountConnectionString,string _containerName)
        {
            Trace.WriteLine($"DEBUG Ensuring container ({_containerName}) exists");
           var blobContainer = GetBlobContainerClient(_storageAccountConnectionString,_containerName);
           blobContainer.CreateIfNotExists();
            return blobContainer;
        }
        public BlobContainerClient GetBlobContainerClient(string _storageAccountConnectionString,string containerName)
        {
            return new BlobContainerClient(_storageAccountConnectionString, containerName);
        }
        public bool ShouldCompressFile(string path, bool compressBlobs)
        {
            if (!compressBlobs)
                return false;

            var ext = Path.GetExtension(path);
            switch (ext)
            {
                case ".cfs":
                case ".fdt":
                case ".fdx":
                case ".frq":
                case ".tis":
                case ".tii":
                case ".nrm":
                case ".tvx":
                case ".tvd":
                case ".tvf":
                case ".prx":
                    return true;
                default:
                    return false;
            }
        }

      
    }
}