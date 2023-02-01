using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Lucene.Providers;
using Examine.Lucene.Search;
using Examine.Search;
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

                var query = searcher.CreateQuery("cOntent", BooleanOperation.And, searcher.LuceneAnalyzer,
                    searchOptions: new LuceneSearchOptions
                    {
                        Similarity = LuceneSearchOptionsSimilarities.ExamineDefault
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

                var query = searcher.CreateQuery("cOntent", BooleanOperation.And, searcher.LuceneAnalyzer,
                    searchOptions: new LuceneSearchOptions
                    {
                        Similarity = LuceneSearchOptionsSimilarities.BM25
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

                var query = searcher.CreateQuery("cOntent", BooleanOperation.And, searcher.LuceneAnalyzer,
                    searchOptions: new LuceneSearchOptions
                    {
                        Similarity = LuceneSearchOptionsSimilarities.LMDirichlet
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

                var query = searcher.CreateQuery("cOntent", BooleanOperation.And, searcher.LuceneAnalyzer,
                    searchOptions: new LuceneSearchOptions
                    {
                        Similarity = LuceneSearchOptionsSimilarities.LMJelinekMercerTitle
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

                var query = searcher.CreateQuery("cOntent", BooleanOperation.And, searcher.LuceneAnalyzer,
                    searchOptions: new LuceneSearchOptions
                    {
                        Similarity = LuceneSearchOptionsSimilarities.LMJelinekMercerLongText
                    }).All();

                Console.WriteLine(query);

                var results = query.Execute();

                Assert.AreEqual(3, results.TotalItemCount);
            }
        }

    }
}
