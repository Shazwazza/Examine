using System.Configuration;
using System.IO;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Store;
using Microsoft.Azure.Storage;
using Microsoft.Extensions.Logging;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.AzureDirectory
{
    /// <summary>
    /// The <see cref="IDirectoryFactory"/> for storing master index data in Blob storage for use on the server that can actively write to the index
    /// </summary>
    public class AzureDirectoryFactory : SyncTempEnvDirectoryFactory, IDirectoryFactory
    {
        private readonly ILogger<AzureDirectoryFactory> _logger;
        private readonly SyncMutexManager _syncMutexManager;
        private readonly bool _isReadOnly;
        
        public AzureDirectoryFactory(ILoggerFactory loggerFactory, SyncMutexManager syncMutexManager, bool isReadOnly)
            : base(loggerFactory, syncMutexManager)
        {
            _logger = loggerFactory.CreateLogger<AzureDirectoryFactory>();
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
        /// <param name="luceneIndexFolder">
        /// The lucene index folder.
        /// </param>
        /// <returns>
        /// The <see cref="Lucene.Net.Store.Directory"/>.
        /// </returns>
        public override Directory CreateDirectory(DirectoryInfo luceneIndexFolder)
        {
            var indexFolder = luceneIndexFolder;
            var tempFolder = GetLocalStorageDirectory(indexFolder);

            return new AzureDirectory(
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings[ConfigStorageKey]),
                ConfigurationManager.AppSettings[ConfigContainerKey],
                tempFolder,
                new SimpleFSDirectory(tempFolder),
                _syncMutexManager,
                rootFolder: luceneIndexFolder.Name,
                isReadOnly: _isReadOnly);
        }
        // Explicit implementation, see https://github.com/Shazwazza/Examine/pull/153
        Lucene.Net.Store.Directory IDirectoryFactory.CreateDirectory(DirectoryInfo luceneIndexFolder) => CreateDirectory(luceneIndexFolder);
    }
}
