using System;
using System.IO;
using System.Security;
using System.Web;
using Examine.LuceneEngine.Providers;
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
            var appDomainHash = HttpRuntime.AppDomainAppId.GenerateHash();
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
        /// Gets the index path name from the Dir Info object. By default Examine will store the index files into a folder like:
        /// "External/Index" but we want to exact the "External" part. We cannot guarantee that this is how the index files are stored
        /// so we'll try to extract it and if we can't we'll have to hash the whole path
        /// </summary>
        /// <param name="indexPath"></param>
        /// <returns></returns>
        private static string GetIndexPathName(DirectoryInfo indexPath)
        {
            var parts = indexPath.FullName.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0 && string.Equals(parts[parts.Length - 1], "Index", StringComparison.OrdinalIgnoreCase))
            {
                //in theory this would be the Index name
                return parts[parts.Length - 2];
            }
            return indexPath.FullName.GenerateHash();
        }

    }
}