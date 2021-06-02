using System;
using System.IO;
using System.Threading;
using Examine.Lucene.Providers;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

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
        private bool _disposedValue;
        private Directory _directory;

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

        public virtual Directory CreateDirectory(LuceneIndex index, bool forceUnlock)
            => LazyInitializer.EnsureInitialized(ref _directory,
                () =>
                {
                    var indexFolder = new DirectoryInfo(Path.Combine(BaseDir.FullName, index.Name));

                    DirectoryInfo tempFolder = GetLocalStorageDirectory(indexFolder);

                    var simpleFsDirectory = new SimpleFSDirectory(
                        tempFolder,
                        _lockFactory.GetLockFactory(tempFolder));

                    return simpleFsDirectory;
                });

        protected DirectoryInfo GetLocalStorageDirectory(DirectoryInfo indexPath)
        {
            var indexPathName = GetIndexPathName(indexPath);
            var tempPath = GetTempPath(_applicationIdentifier);
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

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
