using System;
using System.IO;
using Microsoft.Extensions.Options;

namespace Examine.Lucene.Directories
{

    /// <summary>
    /// A directory factory used to create an instance of FSDirectory that uses the current %temp% environment variable.
    /// </summary>
    /// <remarks>
    /// This works well for Azure Web Apps directory sync.
    /// </remarks>
    public class TempEnvFileSystemDirectoryFactory : FileSystemDirectoryFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TempEnvFileSystemDirectoryFactory"/> class.
        /// </summary>
        /// <param name="applicationIdentifier">The application identifier used to generate a unique path.</param>
        /// <param name="lockFactory">The lock factory used for directory locking.</param>
        /// <param name="indexOptions">The options monitor for Lucene directory index options.</param>
        public TempEnvFileSystemDirectoryFactory(
            IApplicationIdentifier applicationIdentifier,
            ILockFactory lockFactory,
            IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions)
            : base(new DirectoryInfo(GetTempPath(applicationIdentifier)), lockFactory, indexOptions)
        {
        }

        /// <summary>
        /// Gets a temp path for examine indexes.
        /// </summary>
        /// <param name="applicationIdentifier">The application identifier used to generate a unique path.</param>
        /// <returns>The temporary path for examine indexes.</returns>
        public static string GetTempPath(IApplicationIdentifier applicationIdentifier)
        {
            var appDomainHash = applicationIdentifier.GetApplicationUniqueIdentifier().GenerateHash();

            var cachePath = Path.Combine(
                Path.GetTempPath(),
                "ExamineIndexes",
                // Include the appdomain hash as a safety check, for example if a website is moved from worker A to worker B and then back
                // to worker A again, in theory the %temp% folder should already be empty but we really want to make sure that it's not
                // utilizing an old index.
                appDomainHash);

            return cachePath;
        }
    }
}
