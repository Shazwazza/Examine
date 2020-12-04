using System;
using System.IO;
using System.Web;
using Examine.LuceneEngine.DeletePolicies;
using Examine.LuceneEngine.Directories;
using Examine.LuceneEngine.MergePolicies;
using Examine.LuceneEngine.MergeShedulers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Examine.AzureDirectory
{

    /// <summary>
    /// The <see cref="IDirectoryFactory"/> for storing master index data in Blob storage for user on the server that only reads from the index
    /// </summary>
    public class ReadOnlyAzureDirectoryFactory : AzureDirectoryFactory, IDirectoryFactory
    {
        private readonly bool _isReadOnly = true;
        private ILogger _logger;
        public ReadOnlyAzureDirectoryFactory()
        {
            _logger = NullLogger.Instance;
        }
        public ReadOnlyAzureDirectoryFactory( ILogger logger)
        {
            _logger = logger ?? NullLogger.Instance;
        }
        public override Lucene.Net.Store.Directory CreateDirectory(DirectoryInfo luceneIndexFolder)
        {
            var indexFolder = luceneIndexFolder;
            var tempFolder = GetLocalStorageDirectory(indexFolder);
            var indexName = GetIndexPathName(indexFolder);
            var directory = new AzureReadOnlyLuceneDirectory(_logger,
                GetStorageAccountConnectionString(),
                GetContainerName(),
                tempFolder,
                indexName,
                rootFolder: luceneIndexFolder.Name,
                isReadOnly: GetIsReadOnly());
       

            directory.IsReadOnly = _isReadOnly;
            directory.SetMergePolicyAction(e => new NoMergePolicy(e));
            directory.SetMergeScheduler(new NoMergeSheduler());
            directory.SetDeletion(NoDeletionPolicy.INSTANCE);
            return directory;
        }
        protected string GetLocalStorageDirectory(DirectoryInfo indexPath)
        {
            var appDomainHash = HttpRuntime.AppDomainAppId.GenerateHash();
            var cachePath = Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "ExamineIndexes",
                //include the appdomain hash is just a safety check, for example if a website is moved from worker A to worker B and then back
                // to worker A again, in theory the %temp%  folder should already be empty but we really want to make sure that its not
                // utilizing an old index
                appDomainHash);
        
            return cachePath;
        }
        private static string GetIndexPathName(DirectoryInfo indexPath)
        {
            return indexPath.FullName.GenerateHash();
        }
        // Explicit implementation, see https://github.com/Shazwazza/Examine/pull/153
        Lucene.Net.Store.Directory IDirectoryFactory.CreateDirectory(DirectoryInfo luceneIndexFolder) => CreateDirectory(luceneIndexFolder);
    
    }
}