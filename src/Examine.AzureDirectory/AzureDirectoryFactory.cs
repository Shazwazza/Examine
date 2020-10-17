using System.Configuration;
using System.IO;
using Examine.LuceneEngine.Directories;
using Examine.LuceneEngine.MergePolicies;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.Azure.Storage;

namespace Examine.AzureDirectory
{
    /// <summary>
    /// The <see cref="IDirectoryFactory"/> for storing master index data in Blob storage for use on the server that can actively write to the index
    /// </summary>
    public class AzureDirectoryFactory : NoMergePolicySyncTempEnvDirectoryFactory, IDirectoryFactory
    {
        private readonly bool _isReadOnly;

        public MergePolicy GetMergePolicy(IndexWriter writer)
        {
            return new NoMergePolicy(writer);
        } 

        public bool IsReadOnly => _isReadOnly;

        public AzureDirectoryFactory() : base()
        {
        }

        public AzureDirectoryFactory(bool isReadOnly) : base()
        {
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
        /// <param name="indexer">
        /// The indexer.
        /// </param>
        /// <param name="luceneIndexFolder">
        /// The lucene index folder.
        /// </param>
        /// <returns>
        /// The <see cref="Lucene.Net.Store.Directory"/>.
        /// </returns>
        public override Lucene.Net.Store.Directory CreateDirectory(LuceneIndexer indexer, string luceneIndexFolder)
        {
            var indexFolder = new DirectoryInfo(luceneIndexFolder);
            var tempFolder = GetLocalStorageDirectory(indexFolder);

            return new AzureDirectory(
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings[ConfigStorageKey]),                
                ConfigurationManager.AppSettings[ConfigContainerKey],
                new SimpleFSDirectory(tempFolder),
                rootFolder: indexer.IndexSetName,
                isReadOnly: _isReadOnly);
        }

        // Explicit implementation, see https://github.com/Shazwazza/Examine/pull/153
        Lucene.Net.Store.Directory IDirectoryFactory.CreateDirectory(LuceneIndexer indexer, string luceneIndexFolder) => CreateDirectory(indexer, luceneIndexFolder);
    }
}