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
        public void Setup() => _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        [TearDown]
        public void TearDown() => _loggerFactory.Dispose();

        private ILogger<TestIndex> GetLogger() => _loggerFactory.CreateLogger<TestIndex>();

        public TestIndex GetTestIndex(Directory d, Analyzer analyzer, FieldDefinitionCollection fieldDefinitions = null)
            => new TestIndex(GetLogger(), fieldDefinitions, d, analyzer);

        public TestIndex GetTestIndex(IndexWriter writer)
            => new TestIndex(GetLogger(), writer);
    }
}
