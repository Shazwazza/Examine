using System;
using System.IO;

namespace Examine.Lucene.Directories
{

    /// <summary>
    /// A directory factory used to create an instance of FSDirectory that uses the current %temp% environment variable
    /// </summary>
    /// <remarks>
    /// This works well for Azure Web Apps directory sync
    /// </remarks>
    public class TempEnvFileSystemDirectoryFactory : FileSystemDirectoryFactory
    {
        public TempEnvFileSystemDirectoryFactory(
            IApplicationIdentifier applicationIdentifier,
            ILockFactory lockFactory,
            DirectoryInfo baseDir)
            : base(GetLocalStorageDirectory(baseDir, applicationIdentifier), lockFactory)
        {
        }

        public static string GetTempPath(IApplicationIdentifier applicationIdentifier)
        {
            var appDomainHash = applicationIdentifier.GetApplicationUniqueIdentifier().GenerateHash();
            
            var cachePath = Path.Combine(
                Environment.ExpandEnvironmentVariables("%temp%"),
                "ExamineIndexes",
                //include the appdomain hash is just a safety check, for example if a website is moved from worker A to worker B and then back
                // to worker A again, in theory the %temp%  folder should already be empty but we really want to make sure that its not
                // utilizing an old index
                appDomainHash);

            return cachePath;
        }

        private static DirectoryInfo GetLocalStorageDirectory(DirectoryInfo indexPath, IApplicationIdentifier applicationIdentifier)
        {
            var indexPathName = GetIndexPathName(indexPath);
            var tempPath = GetTempPath(applicationIdentifier);
            var tempDir = new DirectoryInfo(Path.Combine(tempPath, indexPathName));

            if (tempDir.Exists == false)
            {
                tempDir.Create();
            }

            return tempDir;
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
