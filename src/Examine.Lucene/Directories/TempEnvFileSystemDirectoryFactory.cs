using System;
using System.IO;
using Microsoft.Extensions.Options;

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
        [Obsolete("Use ctor with all dependencies")]
        public TempEnvFileSystemDirectoryFactory(
            IApplicationIdentifier applicationIdentifier,
            ILockFactory lockFactory)
            : this(applicationIdentifier, lockFactory, new FakeLuceneDirectoryIndexOptionsOptionsMonitor())
        {
        }

        public TempEnvFileSystemDirectoryFactory(
            IApplicationIdentifier applicationIdentifier,
            ILockFactory lockFactory,
            IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions)
            : base(new DirectoryInfo(GetTempPath(applicationIdentifier)), lockFactory, indexOptions)
        {
        }

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
