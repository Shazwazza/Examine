using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Examine.Logging;
using Lucene.Net.Store;

namespace Examine.RemoteDirectory
{
    public class RemoteReadOnlyLuceneSyncDirectory : RemoteSyncDirectory
    {
        private readonly string _cacheDirectoryPath;
        private readonly string _cacheDirectoryName;
        private string _oldIndexFolderName;

        public RemoteReadOnlyLuceneSyncDirectory(IRemoteDirectory remoteDirectory,
            string cacheDirectoryPath,
            string cacheDirectoryName,
            ILoggingService loggingService,
            bool compressBlobs = false) : base(remoteDirectory,loggingService, compressBlobs)
        {
            _cacheDirectoryPath = cacheDirectoryPath;
            _cacheDirectoryName = cacheDirectoryName;
            IsReadOnly = true;
            if (CacheDirectory == null)
            {
                LoggingService.Log(new LogEntry(LogLevel.Error,null,$"CacheDirectory null. Creating or rebuilding cache"));

                CreateOrReadCache();
            }
            else
            {
                CheckDirty();
            }
        }
        public RemoteReadOnlyLuceneSyncDirectory(IRemoteDirectory remoteDirectory,
            string cacheDirectoryPath,
            string cacheDirectoryName,
            bool compressBlobs = false) : base(remoteDirectory, compressBlobs)
        {
            _cacheDirectoryPath = cacheDirectoryPath;
            _cacheDirectoryName = cacheDirectoryName;
            IsReadOnly = true;
            if (CacheDirectory == null)
            {
                LoggingService.Log(new LogEntry(LogLevel.Error,null,$"CacheDirectory null. Creating or rebuilding cache"));

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
            lock (RebuildLock)
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
        public override IndexOutput CreateOutput(string name)
        {
            SetDirty();
            CheckDirty();
            LoggingService.Log(new LogEntry(LogLevel.Info,null,$"Opening output for {_oldIndexFolderName}"));
            return CacheDirectory.CreateOutput(name);
        }
        public override IndexInput OpenInput(string name)
        {
            SetDirty();
            CheckDirty();
            LoggingService.Log(new LogEntry(LogLevel.Info,null,$"Opening input for {_oldIndexFolderName}"));
            return CacheDirectory.OpenInput(name);
        }
        protected override void HandleOutOfSync()
        
        {

            lock (RebuildLock)
            {
                RebuildCache();
                HandleOutOfSyncDirectory();
            }
        }

        //todo: make that as background task. Need input from someone how to handle that correctly as now it is as sync task to avoid issues, but need be change
        public override void RebuildCache()
        {
            lock (RebuildLock)
            {
                //Needs locking
                LoggingService.Log(new LogEntry(LogLevel.Info,null,$"Rebuilding cache"));

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
                    var status = RemoteDirectory.SyncFile(newIndex, file, CompressBlobs);
                    if (!status)
                    {
                        LoggingService.Log(new LogEntry(LogLevel.Error,null,$"Rebuilding cache failed"));
                        newIndex.Dispose();
                    }
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
                        LoggingService.Log(new LogEntry(LogLevel.Error,ex,$"Exception on unlocking old cache index folder"));

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
                        LoggingService.Log(new LogEntry(LogLevel.Error,ex,$"Cleaning of old directory failed."));

                    }
                }

                _oldIndexFolderName = tempDir.Name;
            }
        }
        internal override string[] GetAllBlobFiles()
        {
            lock (RebuildLock)
            {
             return  base.GetAllBlobFiles();
            }
        }
    }
}