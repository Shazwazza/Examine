using Examine.Lucene;
using Examine.Lucene.Directories;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Benchmarks
{
    public abstract class ExamineBaseTest
    {
        protected ILoggerFactory? LoggerFactory { get; private set; }

        public virtual void Setup()
        {
            LoggerFactory = CreateLoggerFactory();
            LoggerFactory.CreateLogger(typeof(ExamineBaseTest)).LogDebug("Initializing test");
        }

        public virtual void TearDown() => LoggerFactory!.Dispose();

        public TestIndex GetTestIndex(
            Directory d,
            Analyzer analyzer,
            FieldDefinitionCollection? fieldDefinitions = null,
            IndexDeletionPolicy? indexDeletionPolicy = null,
            IReadOnlyDictionary<string, IFieldValueTypeFactory>? indexValueTypesFactory = null,
            double nrtTargetMaxStaleSec = 60,
            double nrtTargetMinStaleSec = 1,
            bool nrtEnabled = true)
            => new TestIndex(
            LoggerFactory!,
                Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions
                {
                    FieldDefinitions = fieldDefinitions,
                    DirectoryFactory = new GenericDirectoryFactory(_ => d, true),
                    Analyzer = analyzer,
                    IndexDeletionPolicy = indexDeletionPolicy,
                    IndexValueTypesFactory = indexValueTypesFactory,
#if LocalBuild
                    NrtTargetMaxStaleSec = nrtTargetMaxStaleSec,
                    NrtTargetMinStaleSec = nrtTargetMinStaleSec,
                    NrtEnabled = nrtEnabled 
#endif
                }));

        //public TestIndex GetTestIndex(
        //    IndexWriter writer,
        //    double nrtTargetMaxStaleSec = 60,
        //    double nrtTargetMinStaleSec = 1)
        //    => new TestIndex(
        //    LoggerFactory,
        //        Mock.Of<IOptionsMonitor<LuceneIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneIndexOptions
        //        {
        //            NrtTargetMaxStaleSec = nrtTargetMaxStaleSec,
        //            NrtTargetMinStaleSec = nrtTargetMinStaleSec
        //        }),
        //        writer);

        protected virtual ILoggerFactory CreateLoggerFactory()
            => Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
    }
}
