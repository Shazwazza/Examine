using NUnit.Framework;
using Lucene.Net.Index;
using Microsoft.Extensions.Logging;
using Lucene.Net.Analysis;
using Directory = Lucene.Net.Store.Directory;

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
            => new TestIndex(_loggerFactory, fieldDefinitions, d, analyzer);

        public TestIndex GetTestIndex(IndexWriter writer)
            => new TestIndex(_loggerFactory, writer);
    }
}
