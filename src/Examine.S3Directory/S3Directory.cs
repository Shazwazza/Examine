using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Amazon.S3;
using Amazon.S3.IO;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.S3Directory
{
    /// <summary>
    /// A Lucene directory used to store master index files in blob storage and sync local files to a %temp% fast drive storage
    /// </summary>
    public class S3Directory : Directory
    {
        private readonly bool _isReadOnly;
        private volatile bool _dirty = true;
        private bool _inSync;
        private readonly object _locker = new object();
                
        protected internal readonly string _containerName;
        private string _bucketURL;
        protected internal AmazonS3Client _blobClient;
        private readonly LockFactory _lockFactory;

        /// <summary>
        /// Create an AzureDirectory
        /// </summary>
        /// <param name="storageAccount">storage account to use</param>
        /// <param name="containerName">name of container (folder in blob storage)</param>
        /// <param name="cacheDirectory">local Directory object to use for local cache</param>
        /// <param name="compressBlobs"></param>
        /// <param name="rootFolder">path of the root folder inside the container</param>
        /// <param name="isReadOnly">
        /// By default this is set to false which means that the <see cref="LockFactory"/> created for this directory will be 
        /// a <see cref="MultiIndexLockFactory"/> which will create locks in both the cache and blob storage folders.
        /// If this is set to true, the lock factory will be the default LockFactory configured for the cache directorty.
        /// </param>
        public S3Directory(
            string accessKey,      
            string secretKey,
            string containerName,
            Directory cacheDirectory,
            bool compressBlobs = false,
            string rootFolder = null,
            bool isReadOnly = false)
        {            
            if (accessKey == null) throw new ArgumentNullException(nameof(accessKey));
            if (secretKey == null) throw new ArgumentNullException(nameof(secretKey));
            if (cacheDirectory == null) throw new ArgumentNullException(nameof(cacheDirectory));
            if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(containerName));
            _isReadOnly = isReadOnly;

            CacheDirectory = cacheDirectory;
            _containerName = containerName.ToLower();
            
            _lockFactory = isReadOnly
                ? CacheDirectory.LockFactory
                : new MultiIndexLockFactory(new S3DirectorySimpleLockFactory(this), CacheDirectory.LockFactory);

            if (string.IsNullOrEmpty(rootFolder))
                RootFolder = string.Empty;
            else
            {
                rootFolder = rootFolder.Trim('/');
                RootFolder = rootFolder + "/";
            }

            _blobClient = new AmazonS3Client(accessKey, secretKey);

            EnsureContainer();
            CompressBlobs = compressBlobs;
        }

        public string RootFolder { get; }
        public bool CompressBlobs { get; }
        public Directory CacheDirectory { get; }

        public void ClearCache()
        {
            foreach (string file in CacheDirectory.ListAll())
            {
                CacheDirectory.DeleteFile(file);
            }
        }

        public void EnsureContainer()
        {
            if (!AmazonS3Util.DoesS3BucketExist(_blobClient, _containerName))
            {
                var putBucketRequest = new PutBucketRequest
                {
                    BucketName = _containerName,
                    UseClientRegion = true
                };

                PutBucketResponse putBucketResponse =  _blobClient.PutBucket(putBucketRequest);
                _bucketURL = FindBucketLocationAsync(_blobClient);
            }
            else
            {
                _bucketURL = FindBucketLocationAsync(_blobClient);
            }   
        }
        private string FindBucketLocationAsync(IAmazonS3 client)
        {
            string bucketLocation;
            var request = new GetBucketLocationRequest
            {
                
                BucketName = _containerName
            };
            GetBucketLocationResponse response =  client.GetBucketLocation(request);
            bucketLocation = response.Location.ToString();
            return bucketLocation;
        }
    
        public override string[] ListAll()
        {
            var blobFiles = CheckDirty();

            return _inSync 
                ? CacheDirectory.ListAll() 
                : (blobFiles ?? GetAllBlobFiles());
        }

        private string[] GetAllBlobFiles()
        {
            ListObjectsV2Request request = new ListObjectsV2Request();
            var results = _blobClient.ListObjectsV2(request).S3Objects.Select(x=>x.Key);
            return results.ToArray();
        }

        /// <summary>Returns true if a file with the given name exists. </summary>
        public override bool FileExists(String name)
        {
            CheckDirty();

            if (_inSync)
            {
                try
                {
                    return CacheDirectory.FileExists(name);
                }
                catch (Exception)
                {
                    //revert to checking the master - what implications would this have?
                    try
                    {
                        S3FileInfo s3FileInfo = new S3FileInfo(_blobClient, _containerName, RootFolder+name);

                        return s3FileInfo.Exists;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            try
            {
                S3FileInfo s3FileInfo = new S3FileInfo(_blobClient, _containerName, RootFolder+name);

                return s3FileInfo.Exists;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Returns the time the named file was last modified. </summary>
        public override long FileModified(String name)
        {
            CheckDirty();

            if (_inSync)
            {
                return CacheDirectory.FileModified(name);
            }

            try
            {
                var blob = new S3FileInfo(_blobClient, _containerName, RootFolder+name);;

                if (blob.Exists)
                {
                    var utcDate = blob.LastWriteTimeUtc;

                    //This is the data structure of how the default Lucene FSDirectory returns this value so we want
                    // to be consistent with how Lucene works
                    return (long)utcDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>Set the modified time of an existing file to now. </summary>
        [Obsolete("This is actually never used")]
        public override void TouchFile(string name)
        {
            //just update the cache file - the Lucene source actually never calls this method!
            CacheDirectory.TouchFile(name);
            SetDirty();
        }

        /// <summary>Removes an existing file in the directory. </summary>
        public override void DeleteFile(string name)
        {
            //We're going to try to remove this from the cache directory first,
            // because the IndexFileDeleter will call this file to remove files 
            // but since some files will be in use still, it will retry when a reader/searcher
            // is refreshed until the file is no longer locked. So we need to try to remove 
            // from local storage first and if it fails, let it keep throwing the IOExpception
            // since that is what Lucene is expecting in order for it to retry.
            //If we remove the main storage file first, then this will never retry to clean out
            // local storage because the FileExist method will always return false.
            if (CacheDirectory.FileExists(name + ".blob"))
            {
                CacheDirectory.DeleteFile(name + ".blob");
            }

            if (CacheDirectory.FileExists(name))
            {
                CacheDirectory.DeleteFile(name);
                SetDirty();
            }

            //if we are readonly, then we are only modifying local storage
            if (_isReadOnly) return;

            //if we've made it this far then the cache directly file has been successfully removed so now we'll do the master
            
            var blob = new S3FileInfo(_blobClient, _containerName, RootFolder+name);;

            if (blob.Exists)
            {
                blob.Delete();
            }

            SetDirty();

            Trace.WriteLine($"DELETE https://{_containerName}.s3.amazonaws.com+/{RootFolder+name}");
        }

        
        /// <summary>Returns the length of a file in the directory. </summary>
        public override long FileLength(string name)
        {
            CheckDirty();

            if (_inSync)
            {
                return CacheDirectory.FileLength(name);
            }

            S3FileInfo s3FileInfo = new S3FileInfo(_blobClient, _containerName, RootFolder+name);
            return s3FileInfo.Length; 
        }

        /// <summary>Creates a new, empty file in the directory with the given name.
        /// Returns a stream writing this file. 
        /// </summary>
        public override IndexOutput CreateOutput(string name)
        {
            SetDirty();

            //if we are readonly, then we are only modifying local storage
            if (_isReadOnly)
            {
                return CacheDirectory.CreateOutput(name);
            }

            S3FileInfo s3FileInfo = new S3FileInfo(_blobClient, _containerName, RootFolder+name);

            return new S3IndexOutput(this, s3FileInfo, name);
        }

        /// <summary>Returns a stream reading an existing file. </summary>
        public override IndexInput OpenInput(string name)
        {
            CheckDirty();
            
            if (_inSync)
            {
                try
                {
                    return CacheDirectory.OpenInput(name);
                }
                catch (FileNotFoundException)
                {
                    //if it's not found then we need to re-read from blob so were not in sync
                    SetDirty();
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Could not get local file though we are marked as inSync, reverting to try blob storage; " + ex);
                }
            }

            //try to sync the file from blob storage

            try
            {
                S3FileInfo s3FileInfo = new S3FileInfo(_blobClient, _containerName, RootFolder+name);
               
                return new S3IndexInput(this, s3FileInfo);
            }
            catch (AmazonS3Exception err)
            {
                throw new FileNotFoundException(name, err);
            }
        }

        /// <summary>Construct a {@link Lock}.</summary>
        /// <param name="name">the name of the lock file
        /// </param>
        public override Lock MakeLock(string name)
        {
            return _lockFactory.MakeLock(name);            
        }

        public override void ClearLock(string name)
        {
            _lockFactory.ClearLock(name);            
        }

        public override LockFactory LockFactory => _lockFactory;

        protected override void Dispose(bool disposing)
        {
            _blobClient = null;
        }

        /// <summary> Return a string identifier that uniquely differentiates
        /// this Directory instance from other Directory instances.
        /// This ID should be the same if two Directory instances
        /// (even in different JVMs and/or on different machines)
        /// are considered "the same index".  This is how locking
        /// "scopes" to the right index.
        /// </summary>
        public override string GetLockId()
        {
            return string.Concat(base.GetLockId(), CacheDirectory.GetLockId());
        }

        public virtual bool ShouldCompressFile(string path)
        {
            if (!CompressBlobs)
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
            };
        }
        
        /// <summary>
        /// Checks dirty flag and sets the _inSync flag after querying the blob strorage vs local storage segment gen
        /// </summary>
        /// <returns>
        /// If _dirty is true and blob storage files are looked up, this will return those blob storage files, this is a performance gain so
        /// we don't double query blob storage.
        /// </returns>
        private string[] CheckDirty()
        {
            if (_dirty)
            {
                lock (_locker)
                {
                    //double check locking
                    if (_dirty)
                    {
                        //these methods don't throw exceptions, will return -1 if something has gone wrong
                        // in which case we'll consider them not in sync
                        var blobFiles = GetAllBlobFiles();
                        var masterSeg = SegmentInfos.GetCurrentSegmentGeneration(blobFiles);
                        var localSeg = SegmentInfos.GetCurrentSegmentGeneration(CacheDirectory);
                        _inSync = masterSeg == localSeg && masterSeg != -1;
                        _dirty = false;
                        return blobFiles;
                    }
                }
            }

            return null;
        }
        
        private void SetDirty()
        {
            if (!_dirty)
            {
                lock (_locker)
                {
                    _dirty = true;
                }
            }
        }
    }

}
