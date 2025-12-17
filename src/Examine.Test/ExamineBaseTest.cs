using System.Collections.Generic;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Lucene.Net.Analysis;
using Lucene.Net.Facet;
using Lucene.Net.Facet.Taxonomy.Directory;
using Lucene.Net.Index;
using Lucene.Net.Replicator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Test
{
    public abstract class ExamineBaseTest
    {
        protected ILoggerFactory LoggerFactory  =>  CreateLoggerFactory();

        [SetUp]
        public virtual void Setup()
        {
        }

        [TearDown]
        public virtual void TearDown() => LoggerFactory.Dispose();

        public TestIndex GetTestIndex(
            Directory luceneDir,
            Directory taxonomyDir,
            Analyzer analyzer,
            FieldDefinitionCollection? fieldDefinitions = null,
            IndexDeletionPolicy? indexDeletionPolicy = null,
            IReadOnlyDictionary<string, IFieldValueTypeFactory>? indexValueTypesFactory = null,
            double nrtTargetMaxStaleSec = 60,
            double nrtTargetMinStaleSec = 1,
            bool nrtEnabled = true,
            FacetsConfig? facetsConfig = null)
            => new TestIndex(
                LoggerFactory,
                Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions
                {
                    FieldDefinitions = fieldDefinitions ?? new FieldDefinitionCollection(),
                    DirectoryFactory = GenericDirectoryFactory.FromExternallyManaged(_ => luceneDir, _ => taxonomyDir),
                    Analyzer = analyzer,
                    IndexDeletionPolicy = indexDeletionPolicy,
                    IndexValueTypesFactory = indexValueTypesFactory,
                    NrtTargetMaxStaleSec = nrtTargetMaxStaleSec,
                    NrtTargetMinStaleSec = nrtTargetMinStaleSec,
                    NrtEnabled = nrtEnabled,
                    FacetsConfig = facetsConfig ?? new FacetsConfig()
                }));

        public TestIndex GetTestIndex(
            IndexWriter writer,
            SnapshotDirectoryTaxonomyIndexWriterFactory taxonomyWriterFactory,
            double nrtTargetMaxStaleSec = 60,
            double nrtTargetMinStaleSec = 1)
            => new TestIndex(
                LoggerFactory,
                Mock.Of<IOptionsMonitor<LuceneIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneIndexOptions
                {
                    NrtTargetMaxStaleSec = nrtTargetMaxStaleSec,
                    NrtTargetMinStaleSec = nrtTargetMinStaleSec
                }),
                writer,
                taxonomyWriterFactory);

        protected virtual ILoggerFactory CreateLoggerFactory()
            => Microsoft.Extensions.Logging.LoggerFactory.Create(
                builder => builder.AddConsole()
                    .SetMinimumLevel(
#if DEBUG
                        LogLevel.Debug
#else
                        LogLevel.Information
#endif
                    ));
    }
}
