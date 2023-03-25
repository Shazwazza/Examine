using System;
using System.Collections;
using System.Collections.Generic;
using Examine.Lucene.Providers;
using Examine.Lucene.Search;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search.Similarities;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene.Search
{
    [TestFixture]
    public class SimilarityTests : ExamineBaseTest
    {
        [Test]
        public void Default_Similarity()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.Integer))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "cOntent",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "cOntent",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "cOntent",
                        new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia"})
                    });

                var searcher = (BaseLuceneSearcher)indexer.Searcher;

                var query = searcher.CreateQuery("cOntent",
                    searchOptions: new LuceneSearchOptions
                    {
                        SimilarityName = ExamineLuceneSimilarityNames.ExamineDefault
                    }).All();

                Console.WriteLine(query);

                var results = query.Execute();

                Assert.AreEqual(3, results.TotalItemCount);
            }
        }

        [Test]
        public void BM25_Similarity()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.Integer))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "cOntent",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "cOntent",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "cOntent",
                        new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia"})
                    });

                var searcher = (BaseLuceneSearcher)indexer.Searcher;

                var query = searcher.CreateQuery("cOntent",
                    searchOptions: new LuceneSearchOptions
                    {
                        SimilarityName = ExamineLuceneSimilarityNames.BM25
                    }).All();

                Console.WriteLine(query);

                var results = query.Execute();

                Assert.AreEqual(3, results.TotalItemCount);
            }
        }


        [Test]
        public void LMDirichlet_Similarity()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.Integer))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "cOntent",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "cOntent",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "cOntent",
                        new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia"})
                    });

                var searcher = (BaseLuceneSearcher)indexer.Searcher;

                var query = searcher.CreateQuery("cOntent",
                    searchOptions: new LuceneSearchOptions
                    {
                        SimilarityName = ExamineLuceneSimilarityNames.LMDirichlet
                    }).All();

                Console.WriteLine(query);

                var results = query.Execute();

                Assert.AreEqual(3, results.TotalItemCount);
            }
        }

        [Test]
        public void LMJelinekMercer_Title_Similarity()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.Integer))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "cOntent",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "cOntent",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "cOntent",
                        new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia"})
                    });

                var searcher = (BaseLuceneSearcher)indexer.Searcher;

                var query = searcher.CreateQuery("cOntent",
                    searchOptions: new LuceneSearchOptions
                    {
                        SimilarityName = ExamineLuceneSimilarityNames.LMJelinekMercerTitle
                    }).All();

                Console.WriteLine(query);

                var results = query.Execute();

                Assert.AreEqual(3, results.TotalItemCount);
            }
        }

        [Test]
        public void LMJelinekMercer_LongText_Similarity()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.Integer))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "cOntent",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "cOntent",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "cOntent",
                        new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia"})
                    });

                var searcher = (BaseLuceneSearcher)indexer.Searcher;

                var query = searcher.CreateQuery("cOntent",
                    searchOptions: new LuceneSearchOptions
                    {
                        SimilarityName = ExamineLuceneSimilarityNames.LMJelinekMercerLongText
                    }).All();

                Console.WriteLine(query);

                var results = query.Execute();

                Assert.AreEqual(3, results.TotalItemCount);
            }
        }

        internal class TestPerFieldSimilarityWrapper : PerFieldSimilarityWrapper
        {
            private readonly Similarity _defaultSimilarity;
            private readonly IDictionary<string, Similarity> _fieldSimilarities;

            public TestPerFieldSimilarityWrapper(Similarity defaultSimilarity, IDictionary<string, Similarity> fieldSimilarities)
            {
                _defaultSimilarity = defaultSimilarity;
                _fieldSimilarities = fieldSimilarities;
            }

            public override Similarity Get(string field)
            {
                if (_fieldSimilarities.TryGetValue(field, out var similarity))
                {
                    return similarity;
                }
                return _defaultSimilarity;
            }
        }

        [Test]
        public void Custom_PerField_Similarity()
        {
            Dictionary<string, Similarity> fieldSimilarities = new Dictionary<string, Similarity>(StringComparer.OrdinalIgnoreCase)
                {
                    { "title", LuceneSearchOptionsSimilarities.LMJelinekMercerTitle },
                    { "bodyText", LuceneSearchOptionsSimilarities.LMJelinekMercerLongText }
                };
            DictionaryPerFieldSimilarityWrapper testSimilarity = new DictionaryPerFieldSimilarityWrapper(fieldSimilarities, LuceneSearchOptionsSimilarities.BM25);

            var sim = new LuceneSimilarityDefinition("dictionarySim", testSimilarity);
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("parentID", FieldDefinitionTypes.Integer))))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "cOntent",
                        new { nodeName = "location 1", title = "Africa location", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "cOntent",
                        new { nodeName = "location 2",title = "Canada location", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "cOntent",
                        new { nodeName = "location 3",title = "Australia location", bodyText = "Sydney is the capital of NSW in Australia"})
                    });

                var searcher = (BaseLuceneSearcher)indexer.Searcher;
              

                var query = searcher.CreateQuery("cOntent",
                    searchOptions: new LuceneSearchOptions
                    {
                        SimilarityName = "dictionarySim"
                    }).All();

                Console.WriteLine(query);

                var results = query.Execute();

                Assert.AreEqual(3, results.TotalItemCount);
            }
        }
    }
}
