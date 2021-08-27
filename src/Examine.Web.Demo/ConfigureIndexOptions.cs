using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Examine.Lucene;
using Examine.Lucene.Analyzers;
using Examine.Lucene.Directories;
using Examine.Lucene.Indexing;
using Lucene.Net.Index;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Examine.Web.Demo
{
    /// <summary>
    /// Configures the index options to construct the Examine indexes
    /// </summary>
    public sealed class ConfigureIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
    {
        private readonly SyncedFileSystemDirectoryFactory _syncFileSystemDirectoryFactory;
        private readonly ILoggerFactory _loggerFactory;

        public ConfigureIndexOptions(
            SyncedFileSystemDirectoryFactory syncFileSystemDirectoryFactory,
            ILoggerFactory loggerFactory)
        {
            _syncFileSystemDirectoryFactory = syncFileSystemDirectoryFactory;
            _loggerFactory = loggerFactory;
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
                case "MyIndex":
                    options.IndexValueTypesFactory = new Dictionary<string, IFieldValueTypeFactory>
                    {
                        ["phone"] = new DelegateFieldValueTypeFactory(name =>
                                        new GenericAnalyzerFieldValueType(
                                            name,
                                            _loggerFactory,
                                            new PatternAnalyzer(@"\d{3}\s\d{3}\s\d{4}", 0)))
                    };
                    options.FieldDefinitions.AddOrUpdate(new FieldDefinition("phone", "phone"));
                    break;
            }
        }

        public void Configure(LuceneDirectoryIndexOptions options)
            => throw new NotImplementedException("This is never called and is just part of the interface");
    }

    
}
