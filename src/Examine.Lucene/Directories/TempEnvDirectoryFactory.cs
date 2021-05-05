using System;
using System.IO;
using Lucene.Net.Store;

namespace Examine.Lucene.Directories
{

    /// <summary>
    /// A directory factory used to create an instance of FSDirectory that uses the current %temp% environment variable
    /// </summary>
    /// <remarks>
    /// This works well for Azure Web Apps directory sync
    /// </remarks>
    public class TempEnvDirectoryFactory : IDirectoryFactory
    {
        private readonly IApplicationIdentifier _applicationIdentifier;
        private readonly ILockFactory _lockFactory;

        public DirectoryInfo BaseDir { get; }

        public TempEnvDirectoryFactory(
            IApplicationIdentifier applicationIdentifier,
            ILockFactory lockFactory,
            DirectoryInfo baseDir)
        {
            _applicationIdentifier = applicationIdentifier;
            _lockFactory = lockFactory;
            BaseDir = baseDir;
        }
        
        public virtual global::Lucene.Net.Store.Directory CreateDirectory(string indexName)
        {
            var indexFolder = new DirectoryInfo(Path.Combine(BaseDir.FullName, indexName));

            var tempFolder = GetLocalStorageDirectory(indexFolder);

            var simpleFsDirectory = new SimpleFSDirectory(tempFolder);
            simpleFsDirectory.SetLockFactory(_lockFactory.GetLockFactory(tempFolder));
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
        private static string GetIndexPathName(DirectoryInfo indexPath) => indexPath.FullName.GenerateHash();

    }
}
