using NUnit.Framework;
using Lucene.Net.Index;
using Microsoft.Extensions.Logging;
using Lucene.Net.Analysis;
using Directory = Lucene.Net.Store.Directory;
using Microsoft.Extensions.Options;
using Examine.Lucene;
using Moq;

namespace Examine.Test
{
    public abstract class ExamineBaseTest
    {
        private ILoggerFactory _loggerFactory;

        [SetUp]
        public virtual void Setup()
        {
            _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            _loggerFactory.CreateLogger(typeof(ExamineBaseTest)).LogDebug("Initializing test");
        }

        [TearDown]
        public virtual void TearDown() => _loggerFactory.Dispose();

        public TestIndex GetTestIndex(Directory d, Analyzer analyzer, FieldDefinitionCollection fieldDefinitions = null)
            => new TestIndex(
                _loggerFactory,
                Mock.Of<IOptionsSnapshot<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions
                {
                    FieldDefinitions = fieldDefinitions,
                    IndexDirectory = d,
                    Analyzer = analyzer
                }));

        public TestIndex GetTestIndex(IndexWriter writer)
            => new TestIndex(
                _loggerFactory,
                Mock.Of<IOptionsSnapshot<LuceneIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneIndexOptions()),
                writer);
    }
}