﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Azure.Storage.Blobs;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.AzureDirectory
{
    public class AzureReadOnlyLuceneDirectory : AzureLuceneDirectory
    {
        private readonly string _cacheDirectoryPath;
        private readonly string _cacheDirectoryName;
        private string OldIndexFolderName;

        public AzureReadOnlyLuceneDirectory(
            string storageAccount,
            string containerName,
            string cacheDirectoryPath,
            string cacheDirectoryName,
            bool compressBlobs = false,
            string rootFolder = null) : base(storageAccount, containerName,null,compressBlobs,rootFolder,true)
        {
            _cacheDirectoryPath = cacheDirectoryPath;
            _cacheDirectoryName = cacheDirectoryName;
            if (CacheDirectory == null)
            {
                Trace.WriteLine("INFO CacheDirectory null. Creating or rebuilding cache");
                CreateOrReadCache();
            }
            else
            {
                CheckDirty();
            }
        }

        protected override void GuardCacheDirectory(Lucene.Net.Store.Directory cacheDirectory)
        {
            //Do nothing
        }
        private void CreateOrReadCache()
        {
            var indexParentFolder = new DirectoryInfo(
                Path.Combine(_cacheDirectoryPath,
                    _cacheDirectoryName));
            if (indexParentFolder.Exists)
            {
                var subDirectories = indexParentFolder.GetDirectories();
                if (subDirectories.Any())
                {
                    var directory = subDirectories.FirstOrDefault();
                    OldIndexFolderName = directory.Name;
                    CacheDirectory = new SimpleFSDirectory(directory);
                    _lockFactory = CacheDirectory.LockFactory;
                }
                else
                {
                    RebuildCache();
                }
            }
            else
            {
                RebuildCache();
                
            }
        }

        public void ResyncCache()
        {
            Trace.WriteLine($"INFO Rebuilding index cache {RootFolder}");
            var manifest = this.GetMostRecentManifest();
            if (manifest != null)
            {
                //no files
                return;
                //SyncManifestFiles(manifest);
            }
            try
            {
                Trace.WriteLine($"Clearing index cache {RootFolder}");
                foreach (string file in CacheDirectory.ListAll())
                {
                    Trace.WriteLine("DEBUG Deleting cache file {file}", file);
                    CacheDirectory.DeleteFile(file);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine($"ERROR {e.ToString()}  Exception thrown while rebuilding cache for {RootFolder}");
            }
            foreach (string file in GetAllBlobFiles())
            {
                if (CacheDirectory.FileExists(file))
                {
                    CacheDirectory.TouchFile(file);
                }
                var blob = GetBlobClient(RootFolder + file);
                SyncFile(blob, file);
            }
        }
        protected override void HandleOutOfSync()
        {
            ResyncCache();
        }
        private object _rebuildLock = new object();
        public override void RebuildCache()
        {
            lock (_rebuildLock)
            {

                //Needs locking
                Trace.WriteLine("INFO Rebuilding cache");
                var tempDir = new DirectoryInfo(
                    Path.Combine(_cacheDirectoryPath,
                        _cacheDirectoryName, DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssfffffff")));
                if (tempDir.Exists == false)
                    tempDir.Create();
                Lucene.Net.Store.Directory newIndex = new SimpleFSDirectory(tempDir);
                foreach (string file in GetAllBlobFiles())
                {
                    //   newIndex.TouchFile(file);
                    if ("write.lock".Equals(file))
                    {
                        continue;
                    }
                    var blob = _blobContainer.GetBlobClient(RootFolder + file);
                    SyncFile(newIndex, blob, file);
                }

                var oldIndex = CacheDirectory;
                newIndex.Dispose();
                newIndex = new SimpleFSDirectory(tempDir);

                CacheDirectory = newIndex;
                _lockFactory = newIndex.LockFactory;
                if (oldIndex != null)
                {
                    try
                    {
                        oldIndex.ClearLock("write.lock");
                    }
                    catch (Exception ex)
                    {

                    }
                    foreach (var file in oldIndex.ListAll())
                    {
                        if (oldIndex.FileExists(file))
                        {
                            oldIndex.DeleteFile(file);
                        }
                    }
                    oldIndex.Dispose();
                    DirectoryInfo oldindex = new DirectoryInfo(Path.Combine(_cacheDirectoryPath,
                        _cacheDirectoryName, OldIndexFolderName));
                    oldindex.Delete();
                }
                OldIndexFolderName = tempDir.Name;
            }
        }
        private void SyncFile(Directory newIndex, BlobClient blob, string fileName)
        {
            if (this.ShouldCompressFile(fileName))
            {
                InflateStream(newIndex, blob, fileName);
            }
            else
            {
                using (var fileStream = new StreamOutput(newIndex.CreateOutput(fileName)))
                {
                    // get the blob
                    blob.DownloadTo(fileStream);
                    fileStream.Flush();
                    Trace.WriteLine($"GET {fileName} RETREIVED {fileStream.Length} bytes");

                }
            }
            // then we will get it fresh into local deflatedName 
            // StreamOutput deflatedStream = new StreamOutput(CacheDirectory.CreateOutput(deflatedName));

        }
        protected void InflateStream(Lucene.Net.Store.Directory newIndex, BlobClient blob, string fileName)
        {
            if (this.ShouldCompressFile(fileName))
            {
                InflateStream(newIndex, blob, fileName);
            }
            else
            {
                using (var fileStream = new StreamOutput(newIndex.CreateOutput(fileName)))
                {
                    // get the blob
                    blob.DownloadTo(fileStream);
                    fileStream.Flush();
                    Trace.WriteLine($"GET {fileName} RETREIVED {fileStream.Length} bytes");

                }

            }
        }

        public override Lock MakeLock(string name)
        {
            return base.MakeLock(name);
        }

        public override void SyncManifest(ExamineIndexWriter writer)
        {
            //DO nothing. Read only
        }
    }
}