using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Examine.Logging;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Directories
{
    /// <summary>
    /// A directory factory used to create an instance of SyncDirectory that uses the current %temp% environment variable
    /// </summary>
    /// <remarks>
    /// This works well for Azure Web Apps directory sync
    /// </remarks>
    public class SyncTempEnvDirectoryFactory : TempEnvDirectoryFactory
    {
        private readonly ILoggingService _loggingService;
        public SyncTempEnvDirectoryFactory() : this(new TraceLoggingService())
        {
        }
        public SyncTempEnvDirectoryFactory(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public override Lucene.Net.Store.Directory CreateDirectory(DirectoryInfo luceneIndexFolder)
        {
            var master = luceneIndexFolder;
            var tempFolder = GetLocalStorageDirectory(master);
            var masterDir = new SimpleFSDirectory(master);
            var cacheDir = new SimpleFSDirectory(tempFolder);
            masterDir.SetLockFactory(DefaultLockFactory(master));
            cacheDir.SetLockFactory(DefaultLockFactory(tempFolder));
            return new SyncDirectory(masterDir, cacheDir, _loggingService);
        }
        
    }
}