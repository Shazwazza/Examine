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
using Lucene.Net.Facet;

namespace Examine.Test
{
    public abstract class ExamineBaseTest
    {
        [SetUp]
        public virtual void Setup()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            loggerFactory.CreateLogger(typeof(ExamineBaseTest)).LogDebug("Initializing test");
        }

        public TestIndex GetTestIndex(Directory d, Analyzer analyzer, FieldDefinitionCollection fieldDefinitions = null, IndexDeletionPolicy indexDeletionPolicy = null, IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypesFactory = null, FacetsConfig facetsConfig = null)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            return new TestIndex(
                loggerFactory,
                Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions
                {
                    FieldDefinitions = fieldDefinitions,
                    DirectoryFactory = new GenericDirectoryFactory(_ => d, null),
                    Analyzer = analyzer,
                    IndexDeletionPolicy = indexDeletionPolicy,
                    IndexValueTypesFactory = indexValueTypesFactory,
                    FacetsConfig = facetsConfig ?? new FacetsConfig()
                }));
        }

        public TestIndex GetTestIndex(IndexWriter writer)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            return new TestIndex(
                loggerFactory,
                Mock.Of<IOptionsMonitor<LuceneIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneIndexOptions()),
                writer);
        }

        public TestIndex GetTaxonomyTestIndex(Directory d, Directory taxonomyDirectory, Analyzer analyzer, FieldDefinitionCollection fieldDefinitions = null, IndexDeletionPolicy indexDeletionPolicy = null, IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypesFactory = null, FacetsConfig? facetsConfig = null)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            return new TestIndex(
                loggerFactory,
                Mock.Of<IOptionsMonitor<LuceneDirectoryIndexOptions>>(x => x.Get(TestIndex.TestIndexName) == new LuceneDirectoryIndexOptions
                {
                    FieldDefinitions = fieldDefinitions,
                    DirectoryFactory = new GenericDirectoryFactory(_ => d, _ => taxonomyDirectory),
                    Analyzer = analyzer,
                    IndexDeletionPolicy = indexDeletionPolicy,
                    IndexValueTypesFactory = indexValueTypesFactory,
                    FacetsConfig = facetsConfig ?? new FacetsConfig(),
                    UseTaxonomyIndex = true
                }));
        }

    }
}
