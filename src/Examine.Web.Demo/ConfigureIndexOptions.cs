using System;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Lucene.Net.Index;
using Microsoft.Extensions.Options;

namespace Examine.Web.Demo
{
    /// <summary>
    /// Configures the index options to construct the Examine indexes
    /// </summary>
    public sealed class ConfigureIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
    {
        private readonly SyncFileSystemDirectoryFactory _syncFileSystemDirectoryFactory;

        public ConfigureIndexOptions(SyncFileSystemDirectoryFactory syncFileSystemDirectoryFactory)
        {
            _syncFileSystemDirectoryFactory = syncFileSystemDirectoryFactory;
        }

        public void Configure(string name, LuceneDirectoryIndexOptions options)
        {

            options.UnlockIndex = true;

            switch (name)
            {
                case "SyncedIndex":
                    // to sync an index the deletion policy must be SnapshotDeletionPolicy
                    options.IndexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());
                    options.DirectoryFactory = _syncFileSystemDirectoryFactory;
                    break;
            }
        }

        public void Configure(LuceneDirectoryIndexOptions options)
            => throw new NotImplementedException("This is never called and is just part of the interface");
    }
}
