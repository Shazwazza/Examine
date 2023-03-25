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
using Examine.Lucene.Search;

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

        public virtual SimilarityDefinitionCollection GetSimilarityDefinitions()
        {
            SimilarityDefinitionCollection collection = new SimilarityDefinitionCollection();
            collection.AddOrUpdate(new ExamineLuceneDefaultSimilarityDefinition());
            collection.AddOrUpdate(new LuceneClassicSimilarityDefinition());
            collection.AddOrUpdate(new LuceneBM25imilarityDefinition());
            collection.AddOrUpdate(new LuceneLMDirichletSimilarityDefinition());
            collection.AddOrUpdate(new LuceneLMJelinekMercerTitleSimilarityDefinition());
            collection.AddOrUpdate(new LuceneLMJelinekMercerLongTextSimilarityDefinition());
            return collection;

        }

        public TestIndex GetTestIndex(Directory d, Analyzer analyzer, FieldDefinitionCollection fieldDefinitions = null, IndexDeletionPolicy indexDeletionPolicy = null, IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypesFactory = null, SimilarityDefinitionCollection similarityDefinitions = null)
            => new TestIndex(
                _loggerFactory,
                Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions
                {
                    FieldDefinitions = fieldDefinitions,
                    DirectoryFactory = new GenericDirectoryFactory(_ => d),
                    Analyzer = analyzer,
                    IndexDeletionPolicy = indexDeletionPolicy,
                    IndexValueTypesFactory = indexValueTypesFactory,
                    SimilarityDefinitions = similarityDefinitions ?? GetSimilarityDefinitions()
                }));

        public TestIndex GetTestIndex(IndexWriter writer)
            => new TestIndex(
                _loggerFactory,
                Mock.Of<IOptionsMonitor<LuceneIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneIndexOptions()),
                writer);
    }
}
