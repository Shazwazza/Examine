using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Examine.LuceneEngine.Directories;
using Microsoft.Azure.Storage.Blob;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.AzureDirectory
{
    public static class AzureHelper
    {
        public static CloudBlobContainer EnsureContainer(this IAzureDirectory directory,CloudBlobClient blobClient, string containerName)
        {
          var blodirectorybContainer = blobClient.GetContainerReference(containerName);
          blodirectorybContainer.CreateIfNotExists();
            return blodirectorybContainer;
        }

        internal static string[] GetAllBlobFiles(this IAzureDirectory directory, CloudBlobContainer blobContainer, string rootFolder)
        {
            var results = from blob in blobContainer.ListBlobs(rootFolder)
                select blob.Uri.AbsolutePath.Substring(blob.Uri.AbsolutePath.LastIndexOf('/') + 1);
            return results.ToArray();
        }
        public static void SyncFile(this IAzureDirectory directory,Directory newIndex, CloudBlockBlob blob, string fileName)
        {
            if (directory.ShouldCompressFile(fileName))
            {
                InflateStream(newIndex,blob,fileName);
            }
            else
            {
                using (var fileStream = new StreamOutput(newIndex.CreateOutput(fileName)))
                {
                    // get the blob
                    blob.DownloadToStream(fileStream);
                    fileStream.Flush();
                    Trace.WriteLine($"GET {fileName} RETREIVED {fileStream.Length} bytes");

                }


            }
            // then we will get it fresh into local deflatedName 
            // StreamOutput deflatedStream = new StreamOutput(CacheDirectory.CreateOutput(deflatedName));
           
        }
        private static void InflateStream(Directory newIndex, CloudBlockBlob blob, string fileName)
        {
      
            using (var deflatedStream = new MemoryStream())
            {
                // get the deflated blob
                blob.DownloadToStream(deflatedStream);
                Trace.WriteLine($"GET {fileName} RETREIVED {deflatedStream.Length} bytes");
                // seek back to begininng
                deflatedStream.Seek(0, SeekOrigin.Begin);
                // open output file for uncompressed contents
                using (var fileStream = new StreamOutput(newIndex.CreateOutput(fileName)))
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
    }
    
}