using System.Configuration;
using System.IO;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.S3Directory
{
    /// <summary>
    /// The <see cref="IDirectoryFactory"/> for storing master index data in Blob storage for use on the server that can actively write to the index
    /// </summary>
    public class S3DirectoryFactory : SyncTempEnvDirectoryFactory
    {
        private readonly bool _isReadOnly;

        public S3DirectoryFactory()
        {
        }

        public S3DirectoryFactory(bool isReadOnly)
        {
            _isReadOnly = isReadOnly;
        }

        /// <summary>
        /// Get/set the config storage key
        /// </summary>
        public static string ConfigAccessKey { get; set; } = "examine:S3AccessKey";
        public static string ConfigSecretKey { get; set; } = "examine:S3SecretKey";
        /// <summary>
        /// Get/set the config container key
        /// </summary>
        public static string ConfigContainerKey { get; set; } = "examine:S3Container";

        /// <summary>
        /// Return the S3Directory.
        /// It stores the master index in S3 storage.
        /// Only a master server can write to it.
        /// For each slave server, the S3 storage index files are synced to the local machine.
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

            return new S3Directory(ConfigurationManager.AppSettings[ConfigAccessKey],
                ConfigurationManager.AppSettings[ConfigSecretKey],                
                ConfigurationManager.AppSettings[ConfigContainerKey],
                new SimpleFSDirectory(tempFolder),
                rootFolder: luceneIndexFolder.Name,
                isReadOnly: _isReadOnly);
        }
        
    }
}