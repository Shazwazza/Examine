using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lucene.Net.Store;

namespace Examine.RemoteDirectory
{
    public class RemoteReadOnlyLuceneSyncDirectory : RemoteSyncDirectory
    {
        private readonly string _cacheDirectoryPath;
        private readonly string _cacheDirectoryName;
        private string _oldIndexFolderName;

        public RemoteReadOnlyLuceneSyncDirectory(
            IRemoteDirectory remoteDirectory,
            string cacheDirectoryPath,
            string cacheDirectoryName,
            bool compressBlobs = false) : base(remoteDirectory, compressBlobs)
        {
            _cacheDirectoryPath = cacheDirectoryPath;
            _cacheDirectoryName = cacheDirectoryName;
            IsReadOnly = true;
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
            lock (_rebuildLock)
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
                        _oldIndexFolderName = directory.Name;
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
        }

        protected override void HandleOutOfSync()
        {
            RebuildCache();
        }

        private object _rebuildLock = new object();
        //todo: make that as background task. Need input from someone how to handle that correctly as now it is as sync task to avoid issues, but need be change
        public override void RebuildCache()
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
                    if (file.EndsWith(".lock"))
                    {
                        continue;
                    }

                    RemoteDirectory.SyncFile(newIndex, file, CompressBlobs);
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
                        if (!string.IsNullOrEmpty(LockFactory.LockPrefix))
                        {
                            oldIndex.ClearLock(LockFactory.LockPrefix + "-write.lock");
                        }
                        else
                        {
                            oldIndex.ClearLock("write.lock");
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Error: {ex.ToString()}");
                    }

                    
                    oldIndex.Dispose();
                    try
                    {
                        DirectoryInfo oldindexDir = new DirectoryInfo(Path.Combine(_cacheDirectoryPath,
                            _cacheDirectoryName, _oldIndexFolderName));
                        foreach (var file in oldindexDir.GetFiles())
                        {
                            file.Delete();
                        }


                        oldindexDir.Delete();
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Error: Cleaning of old directory failed. {ex.ToString()}");
                    }
                }

                _oldIndexFolderName = tempDir.Name;
        }
    }
}