using System.IO;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Directories
{
    /// <summary>
    /// A directory factory used to create an instance of SyncDirectory that uses the current %temp% environment variable
    /// </summary>
    /// <remarks>
    /// This works well for Azure Web Apps directory sync
    /// </remarks>
    public class SyncTempEnvDirectoryFactory : TempEnvDirectoryFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly SyncMutexManager _syncMutexManager;
        private readonly ILockFactory _lockFactory;

        public SyncTempEnvDirectoryFactory(IApplicationIdentifier applicationIdentifier, ILoggerFactory loggerFactory, SyncMutexManager syncMutexManager, ILockFactory lockFactory)
            : base(applicationIdentifier, lockFactory)
        {
            _loggerFactory = loggerFactory;
            _syncMutexManager = syncMutexManager;
            _lockFactory = lockFactory;
        }

        public override global::Lucene.Net.Store.Directory CreateDirectory(DirectoryInfo luceneIndexFolder)
        {
            var master = luceneIndexFolder;
            var tempFolder = GetLocalStorageDirectory(master);
            var masterDir = new SimpleFSDirectory(master);
            var cacheDir = new SimpleFSDirectory(tempFolder);
            masterDir.SetLockFactory(_lockFactory.GetLockFactory(master));
            cacheDir.SetLockFactory(_lockFactory.GetLockFactory(tempFolder));
            return new SyncDirectory(masterDir, cacheDir, _loggerFactory, _syncMutexManager);
        }
        
    }
}
