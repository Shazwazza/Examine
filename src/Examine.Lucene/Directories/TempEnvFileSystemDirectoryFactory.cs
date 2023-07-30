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
        /// <inheritdoc/>
        public TempEnvFileSystemDirectoryFactory(
            IApplicationIdentifier applicationIdentifier,
            ILockFactory lockFactory)
            : base(new DirectoryInfo(GetTempPath(applicationIdentifier)), lockFactory)
        {
        }

        /// <summary>
        /// Gets a temp path for examine indexes
        /// </summary>
        /// <param name="applicationIdentifier"></param>
        /// <returns></returns>
        public static string GetTempPath(IApplicationIdentifier applicationIdentifier)
        {
            var appDomainHash = applicationIdentifier.GetApplicationUniqueIdentifier().GenerateHash();
            
            var cachePath = Path.Combine(
                Path.GetTempPath(),
                "ExamineIndexes",
                //include the appdomain hash is just a safety check, for example if a website is moved from worker A to worker B and then back
                // to worker A again, in theory the %temp%  folder should already be empty but we really want to make sure that its not
                // utilizing an old index
                appDomainHash);

            return cachePath;
        }
    }
}
