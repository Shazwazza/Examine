using System.Configuration;
using System.IO;
using Examine.LuceneEngine.DeletePolicies;
using Examine.LuceneEngine.Directories;
using Examine.LuceneEngine.MergePolicies;
using Examine.LuceneEngine.MergeShedulers;
using Examine.Providers;
using Examine.RemoteDirectory;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;
using static Lucene.Net.Index.IndexWriter;

namespace Examine.AzureDirectory
{
    /// <summary>
    /// The <see cref="IDirectoryFactory"/> for storing master index data in Blob storage for use on the server that can actively write to the index
    /// </summary>
    public class AzureDirectoryFactory : SyncTempEnvDirectoryFactory, IDirectoryFactory
    {
        public AzureDirectoryFactory()
        {
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
        public override Lucene.Net.Store.Directory CreateDirectory(DirectoryInfo luceneIndexFolder)
        {
            var directory = new RemoteSyncDirectory(
                new AzureRemoteDirectory(GetStorageAccountConnectionString(),GetContainerName(), luceneIndexFolder.Name),
                GetLocalCacheDirectory(luceneIndexFolder) );
            directory.SetMergePolicyAction(e => new NoMergePolicy(e));
            directory.SetMergeScheduler(new NoMergeSheduler());
            directory.SetDeletion(new NoDeletionPolicy());
            return directory;
        }


        // Explicit implementation, see https://github.com/Shazwazza/Examine/pull/153
        Lucene.Net.Store.Directory IDirectoryFactory.CreateDirectory(DirectoryInfo luceneIndexFolder) => CreateDirectory(luceneIndexFolder);

        /// <summary>
        /// Gets the Local Cache Lucence Directory
        /// </summary>
        /// <param name="luceneIndexFolder">The lucene index folder.</param>
        /// <returns>The <see cref="Lucene.Net.Store.Directory"/> used by Lucence as the local cache syncDirectory</returns>
        protected virtual Lucene.Net.Store.Directory GetLocalCacheDirectory(DirectoryInfo luceneIndexFolder)
        {
            return new SimpleFSDirectory(luceneIndexFolder);
        }

        /// <summary>
        /// Gets the Cloud Storage Account
        /// </summary>
        /// <remarks>Retrieves connection string from <see cref="ConfigStorageKey"/></remarks>
        /// <returns>CloudStorageAccount</returns>
        protected virtual string GetStorageAccountConnectionString()
        {
            return ConfigurationManager.AppSettings[ConfigStorageKey];
        }

        /// <summary>
        /// Retrieve the container name
        /// </summary>
        /// <remarks>Retrieves the container name from <see cref="ConfigContainerKey"/></remarks>
        /// <returns>Name of the container where the indexes are stored</returns>
        protected virtual string GetContainerName()
        {
            return ConfigurationManager.AppSettings[ConfigContainerKey];
        }
    }
}