using NUnit.Framework;
using Lucene.Net.Index;
using Microsoft.Extensions.Logging;
using Lucene.Net.Analysis;
using Directory = Lucene.Net.Store.Directory;
using Microsoft.Extensions.Options;
using Examine.Lucene;
using Moq;
using Examine.Lucene.Directories;
using System.Collections.Generic;

namespace Examine.Test
{
    public abstract class ExamineBaseTest
    {
        protected ILoggerFactory LoggerFactory { get; private set; }

        [SetUp]
        public virtual void Setup()
        {
            LoggerFactory = CreateLoggerFactory();
            LoggerFactory.CreateLogger(typeof(ExamineBaseTest)).LogDebug("Initializing test");
        }

        [TearDown]
        public virtual void TearDown() => LoggerFactory.Dispose();

        public TestIndex GetTestIndex(
            Directory d,
            Analyzer analyzer,
            FieldDefinitionCollection fieldDefinitions = null,
            IndexDeletionPolicy indexDeletionPolicy = null,
            IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypesFactory = null,
            double nrtTargetMaxStaleSec = 60,
            double nrtTargetMinStaleSec = 1,
            bool nrtEnabled = true)
            => new TestIndex(
                LoggerFactory,
                Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions
                {
                    FieldDefinitions = fieldDefinitions,
                    DirectoryFactory = new GenericDirectoryFactory(_ => d),
                    Analyzer = analyzer,
                    IndexDeletionPolicy = indexDeletionPolicy,
                    IndexValueTypesFactory = indexValueTypesFactory,
                    NrtTargetMaxStaleSec = nrtTargetMaxStaleSec,
                    NrtTargetMinStaleSec = nrtTargetMinStaleSec,
                    NrtEnabled = nrtEnabled
                }));

        public TestIndex GetTestIndex(
            IndexWriter writer,
            double nrtTargetMaxStaleSec = 60,
            double nrtTargetMinStaleSec = 1)
            => new TestIndex(
                LoggerFactory,
                Mock.Of<IOptionsMonitor<LuceneIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneIndexOptions
                {
                    NrtTargetMaxStaleSec = nrtTargetMaxStaleSec,
                    NrtTargetMinStaleSec = nrtTargetMinStaleSec
                }),
                writer);

        protected virtual ILoggerFactory CreateLoggerFactory()
            => Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
    }
}
