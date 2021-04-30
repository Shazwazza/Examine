using System;
using System.IO;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Directories
{

    /// <summary>
    /// A directory factory used to create an instance of FSDirectory that uses the current %temp% environment variable
    /// </summary>
    /// <remarks>
    /// This works well for Azure Web Apps directory sync
    /// </remarks>
    public class TempEnvDirectoryFactory : DirectoryFactory
    {
        private readonly IApplicationIdentifier _applicationIdentifier;

        public TempEnvDirectoryFactory(IApplicationIdentifier applicationIdentifier)
        {
            _applicationIdentifier = applicationIdentifier;
        }
        
        public override Lucene.Net.Store.Directory CreateDirectory(DirectoryInfo luceneIndexFolder)
        {
            var indexFolder = luceneIndexFolder;
            var tempFolder = GetLocalStorageDirectory(indexFolder);

            var simpleFsDirectory = new SimpleFSDirectory(tempFolder);
            simpleFsDirectory.SetLockFactory(DefaultLockFactory(tempFolder));
            return simpleFsDirectory;
        }

        protected DirectoryInfo GetLocalStorageDirectory(DirectoryInfo indexPath)
        {
            var appDomainHash = _applicationIdentifier.GetApplicationUniqueIdentifier().GenerateHash();
            var indexPathName = GetIndexPathName(indexPath);
            var cachePath = Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "ExamineIndexes",
                //include the appdomain hash is just a safety check, for example if a website is moved from worker A to worker B and then back
                // to worker A again, in theory the %temp%  folder should already be empty but we really want to make sure that its not
                // utilizing an old index
                appDomainHash, indexPathName);
            var azureDir = new DirectoryInfo(cachePath);
            if (azureDir.Exists == false)
                azureDir.Create();
            return azureDir;
        }

        /// <summary>
        /// Return a sub folder name to store under the temp folder
        /// </summary>
        /// <param name="indexPath"></param>
        /// <returns>
        /// A hash value of the original path
        /// </returns>
        private static string GetIndexPathName(DirectoryInfo indexPath)
        {
            return indexPath.FullName.GenerateHash();
        }

    }
}
