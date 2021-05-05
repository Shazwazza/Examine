using System.Configuration;
using System.IO;
using Examine.Lucene.Directories;
using Lucene.Net.Store;
using Microsoft.Azure.Storage;
using Microsoft.Extensions.Logging;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.AzureDirectory
{
    /// <summary>
    /// The <see cref="IDirectoryFactory"/> for storing master index data in Blob storage for use on the server that can actively write to the index
    /// </summary>
    public class AzureDirectoryFactory : SyncTempEnvDirectoryFactory, IDirectoryFactory
    {
        private readonly ILogger<AzureDirectory> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly SyncMutexManager _syncMutexManager;
        private readonly bool _isReadOnly;
        
        public AzureDirectoryFactory(
            IApplicationIdentifier applicationIdentifier,
            ILoggerFactory loggerFactory,
            SyncMutexManager syncMutexManager,
            ILockFactory lockFactory,
            bool isReadOnly,
            DirectoryInfo baseDir)
            : base(applicationIdentifier, loggerFactory, syncMutexManager, lockFactory, baseDir)
        {
            _logger = loggerFactory.CreateLogger<AzureDirectory>();
            _loggerFactory = loggerFactory;
            _syncMutexManager = syncMutexManager;
            _isReadOnly = isReadOnly;
        }

        /// <summary>
        /// Get/set the config storage key
        /// </summary>
        public static string ConfigStorageKey { get; set; } = "examine:AzureStorageConnString";

        /// <summary>
        /// Get/set the config container key
        /// </summary>
        public static string ConfigContainerKey { get; set; } = "examine:AzureStorageContainer";

        /// <summary>
        /// Return the AzureDirectory.
        /// It stores the master index in Blob storage.
        /// Only a master server can write to it.
        /// For each slave server, the blob storage index files are synced to the local machine.
        /// </summary>
        public override Directory CreateDirectory(string indexName)
        {
            var luceneIndexFolder = new DirectoryInfo(Path.Combine(BaseDir.FullName, indexName));
            var tempFolder = GetLocalStorageDirectory(luceneIndexFolder);

            return new AzureDirectory(
                _loggerFactory,
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings[ConfigStorageKey]),
                ConfigurationManager.AppSettings[ConfigContainerKey],
                tempFolder,
                new SimpleFSDirectory(tempFolder),
                _syncMutexManager,
                rootFolder: luceneIndexFolder.Name,
                isReadOnly: _isReadOnly);
        }
        // Explicit implementation, see https://github.com/Shazwazza/Examine/pull/153
        Directory IDirectoryFactory.CreateDirectory(string indexName) => CreateDirectory(indexName);
    }
}
