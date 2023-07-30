using System.Linq;
using Examine.Lucene.Suggest;
using Examine.Lucene.Suggest.Directories;
using Examine.Suggest;
using Lucene.Net.Analysis.Standard;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene.Suggest
{
    [TestFixture]
    public class SuggesterApiTests : ExamineBaseTest
    {
        FieldDefinitionCollection _fieldDefinitionCollection;
        SuggesterDefinitionCollection _suggesters;

        [SetUp]
        public void Setup()
        {
            _fieldDefinitionCollection = new FieldDefinitionCollection();
            _fieldDefinitionCollection.AddOrUpdate(new FieldDefinition("nodeName", FieldDefinitionTypes.FullText));
            _fieldDefinitionCollection.AddOrUpdate(new FieldDefinition("bodyText", FieldDefinitionTypes.FullText));

            _suggesters = new SuggesterDefinitionCollection();
            _suggesters.AddOrUpdate(new AnalyzingInfixSuggesterDefinition(ExamineLuceneSuggesterNames.AnalyzingInfixSuggester, new string[] { "nodeName" }, new RAMSuggesterDirectoryFactory()));
            _suggesters.AddOrUpdate(new AnalyzingSuggesterDefinition(ExamineLuceneSuggesterNames.AnalyzingSuggester, new string[] { "nodeName" }));
            _suggesters.AddOrUpdate(new DirectSpellCheckerDefinition(ExamineLuceneSuggesterNames.DirectSpellChecker, new string[] { "nodeName" }));
            _suggesters.AddOrUpdate(new LevensteinDistanceSuggesterDefinition(ExamineLuceneSuggesterNames.DirectSpellChecker_LevensteinDistance, new string[] { "nodeName" }));
            _suggesters.AddOrUpdate(new JaroWinklerDistanceDefinition(ExamineLuceneSuggesterNames.DirectSpellChecker_JaroWinklerDistance, new string[] { "nodeName" }));
            _suggesters.AddOrUpdate(new NGramDistanceSuggesterDefinition(ExamineLuceneSuggesterNames.DirectSpellChecker_NGramDistance, new string[] { "nodeName" }));
            _suggesters.AddOrUpdate(new FuzzySuggesterDefinition(ExamineLuceneSuggesterNames.FuzzySuggester, new string[] { "nodeName" }));
        }

        [Test]
        public void Suggest_Text_AnalyzingInfixSuggester()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, _fieldDefinitionCollection, suggesterDefinitions: _suggesters))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia"}),

                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "locksmiths 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "locksmiths 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "locksmiths 3", bodyText = "Sydney is the capital of NSW in Australia"}),

                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "locomotive 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "locomotive 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "locomotive 3", bodyText = "Sydney is the capital of NSW in Australia"}),

                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "vehicle 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "vehicle 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "vehicle 3", bodyText = "Sydney is the capital of NSW in Australia"}),

                     ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "content localization 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "content localization 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "content localization 3", bodyText = "Sydney is the capital of NSW in Australia"}),


                    });

                ISuggester suggester = indexer.Suggester;

                var query = suggester.CreateSuggestionQuery();

                var results = query.Execute("loc", new LuceneSuggestionOptions(5, ExamineLuceneSuggesterNames.AnalyzingInfixSuggester));
                Assert.IsTrue(results.Count() == 4);
                Assert.IsTrue(results.Any(x => x.Text.Equals("<b>loc</b>ation")));
                Assert.IsTrue(results.Any(x => x.Text.Equals("<b>loc</b>ksmiths")));
                Assert.IsTrue(results.Any(x => x.Text.Equals("<b>loc</b>omotive")));
                Assert.IsTrue(results.Any(x => x.Text.Equals("<b>loc</b>alization")));

                var results2 = query.Execute("loco", new LuceneSuggestionOptions(5, ExamineLuceneSuggesterNames.AnalyzingInfixSuggester));
                Assert.IsTrue(results2.Count() == 1);
                Assert.IsTrue(results2.Any(x => x.Text.Equals("<b>loco</b>motive")));
            }
        }

        [Test]
        public void Suggest_Text_AnalyzingSuggester()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, _fieldDefinitionCollection, suggesterDefinitions: _suggesters))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia"}),

                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "locksmiths 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "locksmiths 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "locksmiths 3", bodyText = "Sydney is the capital of NSW in Australia"}),

                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "locomotive 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "locomotive 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "locomotive 3", bodyText = "Sydney is the capital of NSW in Australia"}),

                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "vehicle 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "vehicle 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "vehicle 3", bodyText = "Sydney is the capital of NSW in Australia"}),

                     ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "content localization 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "content localization 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "content localization 3", bodyText = "Sydney is the capital of NSW in Australia"}),


                    });

                ISuggester suggester = indexer.Suggester;

                var query = suggester.CreateSuggestionQuery();

                var results = query.Execute("loc", new LuceneSuggestionOptions(5, ExamineLuceneSuggesterNames.AnalyzingSuggester));
                Assert.IsTrue(results.Count() == 4);
                Assert.IsTrue(results.Any(x => x.Text.Equals("location")));
                Assert.IsTrue(results.Any(x => x.Text.Equals("locksmiths")));
                Assert.IsTrue(results.Any(x => x.Text.Equals("locomotive")));
                Assert.IsTrue(results.Any(x => x.Text.Equals("localization")));

                var results2 = query.Execute("loco", new LuceneSuggestionOptions(5, ExamineLuceneSuggesterNames.AnalyzingSuggester));
                Assert.IsTrue(results2.Count() == 1);
                Assert.IsTrue(results2.Any(x => x.Text.Equals("locomotive")));
            }
        }

        [Test]
        public void Suggest_Text_FuzzySuggester()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, _fieldDefinitionCollection, suggesterDefinitions: _suggesters))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia"}),

                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "locksmiths 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "locksmiths 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "locksmiths 3", bodyText = "Sydney is the capital of NSW in Australia"}),

                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "locomotive 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "locomotive 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "locomotive 3", bodyText = "Sydney is the capital of NSW in Australia"}),

                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "vehicle 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "vehicle 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "vehicle 3", bodyText = "Sydney is the capital of NSW in Australia"}),

                     ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "content localization 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "content localization 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "content localization 3", bodyText = "Sydney is the capital of NSW in Australia"}),


                    });

                ISuggester suggester = indexer.Suggester;
                var query = suggester.CreateSuggestionQuery();

                var results = query.Execute("loc", new LuceneSuggestionOptions(5, ExamineLuceneSuggesterNames.FuzzySuggester));
                Assert.IsTrue(results.Count() == 4);
                Assert.IsTrue(results.Any(x => x.Text.Equals("location")));
                Assert.IsTrue(results.Any(x => x.Text.Equals("locksmiths")));
                Assert.IsTrue(results.Any(x => x.Text.Equals("locomotive")));
                Assert.IsTrue(results.Any(x => x.Text.Equals("localization")));

                var results2 = query.Execute("loco", new LuceneSuggestionOptions(5, ExamineLuceneSuggesterNames.FuzzySuggester));
                Assert.IsTrue(results.Count() == 4);
                Assert.IsTrue(results.Any(x => x.Text.Equals("location")));
                Assert.IsTrue(results.Any(x => x.Text.Equals("locksmiths")));
                Assert.IsTrue(results.Any(x => x.Text.Equals("locomotive")));
                Assert.IsTrue(results.Any(x => x.Text.Equals("localization")));
            }
        }


        [Test]
        public void Suggest_Text_DirectSpellChecker()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer, _fieldDefinitionCollection, suggesterDefinitions: _suggesters))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia"}),

                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "locksmiths 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "locksmiths 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "locksmiths 3", bodyText = "Sydney is the capital of NSW in Australia"}),

                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "locomotive 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "locomotive 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "locomotive 3", bodyText = "Sydney is the capital of NSW in Australia"}),

                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "vehicle 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "vehicle 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "vehicle 3", bodyText = "Sydney is the capital of NSW in Australia"}),

                     ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "content localization 1", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "content localization 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "content localization 3", bodyText = "Sydney is the capital of NSW in Australia"}),


                    });

                ISuggester suggester = indexer.Suggester;
                var query = suggester.CreateSuggestionQuery();

                var results = query.Execute("logomotave", new LuceneSuggestionOptions(5, ExamineLuceneSuggesterNames.DirectSpellChecker));
                Assert.IsTrue(results.Count() == 1);
                Assert.IsTrue(results.Any(x => x.Text.Equals("locomotive")));

                var results2 = query.Execute("localisation", new LuceneSuggestionOptions(5, ExamineLuceneSuggesterNames.DirectSpellChecker));
                Assert.IsTrue(results2.Count() == 1);
                Assert.IsTrue(results2.Any(x => x.Text.Equals("localization")));
            }
        }
    }
}
