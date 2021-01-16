using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Azure.Storage.Blobs;
using Examine.LuceneEngine.Directories;
using Examine.RemoteDirectory;

namespace Examine.AzureDirectory
{
    public class AzureRemoteDirectory : IRemoteDirectory
    {
        private string _storageAccountConnectionString;
        private readonly string _containerName;
        private readonly string _rootFolderName;
        private BlobContainerClient _blobContainer;

        public AzureRemoteDirectory(string storageAccountConnectionString, string containerName,
            string rootFolderName)
        {
            if (storageAccountConnectionString == null)
                throw new ArgumentNullException(nameof(storageAccountConnectionString));
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(containerName));
            _storageAccountConnectionString = storageAccountConnectionString;
            _containerName = containerName;
            _rootFolderName = NormalizeContainerRootFolder(rootFolderName);
            EnsureContainer(containerName);
        }

        /// <summary>. </summary>
        public void SyncFile(Lucene.Net.Store.Directory directory, string fileName, bool CompressBlobs)
        {
            var blob = _blobContainer.GetBlobClient(_rootFolderName + fileName);
            Trace.WriteLine($"INFO Syncing file {fileName} for {_rootFolderName}");
            // then we will get it fresh into local deflatedName 
            // StreamOutput deflatedStream = new StreamOutput(CacheDirectory.CreateOutput(deflatedName));
            using (var deflatedStream = new MemoryStream())
            {
                // get the deflated blob
                blob.DownloadTo(deflatedStream);

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
                        blob.DownloadTo(fileStream);

                        fileStream.Flush();
#if FULLDEBUG
                        Trace.WriteLine($"GET {fileName} RETREIVED {fileStream.Length} bytes");
#endif
                    }
                }
            }
        }

        public long FileLength(string filename, long lenghtFallback)
        {
            try
            {
                var blob = _blobContainer.GetBlobClient(_rootFolderName + filename);
                var blobProperties = blob.GetProperties();

                // index files may be compressed so the actual length is stored in metadata
                var hasMetadataValue =
                    blobProperties.Value.Metadata.TryGetValue("CachedLength", out var blobLegthMetadata);

                if (hasMetadataValue && long.TryParse(blobLegthMetadata, out var blobLength))
                {
                    return blobLength;
                }

                // fall back to actual blob size
                return blobProperties.Value.ContentLength;
            }
            catch (Exception e)
            {
                //  Sync(name);
                Trace.WriteLine(
                    $"ERROR {e.ToString()}  Exception thrown while retrieving file length of file {filename} for {_rootFolderName}");
                return lenghtFallback;
            }
        }

        public IEnumerable<string> GetAllRemoteFileNames()
        {
            return from blob in _blobContainer.GetBlobs(prefix: RootFolder)
                select blob.Name;
        }

        public void EnsureContainer(string _containerName)
        {
            Trace.WriteLine($"DEBUG Ensuring container ({_containerName}) exists");
            var blobContainer = GetBlobContainerClient(_containerName);
            blobContainer.CreateIfNotExists();
            _blobContainer = blobContainer;
        }

        public BlobContainerClient GetBlobContainerClient(string containerName)
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

        protected string NormalizeContainerRootFolder(string rootFolder)
        {
            if (string.IsNullOrEmpty(rootFolder))
                return string.Empty;
            rootFolder = rootFolder.Trim('/');
            rootFolder = rootFolder + "/";
            return rootFolder;
        }

        public bool FileExists(string filename)
        {
            try
            {
                var blob = _blobContainer.GetBlobClient(filename);
                var response = blob.Exists();
                return response.Value;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ERROR {ex.ToString()} Error while checking if index locked");
                throw;
            }
        }
    }
}