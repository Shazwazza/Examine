using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Suggest;
using Lucene.Net.Analysis.Standard;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene.Suggest
{
    [TestFixture]
    public class SuggesterApiTests : ExamineBaseTest
    {
        [Test]
        public void Suggest_Text_AnalyzingSuggester()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer))
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

                var suggester = indexer.Suggester;
                var query = suggester.CreateSuggestionQuery()
                    .SourceFields(new HashSet<string>(){
                        "nodeName" });

                var results = query.Execute("loc", new SuggestionOptions
                {
                    Top = 5,
                    SuggesterName = "AnalyzingSuggester"
                });
                Assert.IsTrue(results.Count() == 4);
                Assert.IsTrue(results.Any(x => x.Text.Equals("location")));
                Assert.IsTrue(results.Any(x => x.Text.Equals("locksmiths")));
                Assert.IsTrue(results.Any(x => x.Text.Equals("locomotive")));
                Assert.IsTrue(results.Any(x => x.Text.Equals("localization")));

                var results2 = query.Execute("loco", new SuggestionOptions
                {
                    Top = 5,
                    SuggesterName = "AnalyzingSuggester"
                });
                Assert.IsTrue(results2.Count() == 1);
                Assert.IsTrue(results2.Any(x => x.Text.Equals("locomotive")));
            }
        }

        [Test]
        public void Suggest_Text_DirectSpellChecker()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(luceneDir, analyzer))
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

                var suggester = indexer.Suggester;
                var query = suggester.CreateSuggestionQuery()
                    .SourceFields(new HashSet<string>(){
                        "nodeName" });

                var results = query.Execute("logomotave", new SuggestionOptions
                {
                    Top = 5,
                    SuggesterName = "DirectSpellChecker"
                });
                Assert.IsTrue(results.Count() == 1);
                Assert.IsTrue(results.Any(x => x.Text.Equals("locomotive")));

                var results2 = query.Execute("localisation", new SuggestionOptions
                {
                    Top = 5,
                    SuggesterName = "DirectSpellChecker"
                });
                Assert.IsTrue(results2.Count() == 1);
                Assert.IsTrue(results2.Any(x => x.Text.Equals("localization")));
            }
        }
    }
}
