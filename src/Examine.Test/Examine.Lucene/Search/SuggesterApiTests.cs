using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis.Standard;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene.Search
{
    [TestFixture]
    public class SuggesterApiTests : ExamineBaseTest
    {
        [Test]
        public void Suggest_Text()
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
                        new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia"})
                    });

                var suggester = indexer.Suggester;
                var query = suggester.CreateSuggestionQuery()
                    .SourceFields(new HashSet<string>(){
                        "nodeName" });

                var results = query.Execute("loc", new Suggest.SuggestionOptions
                {
                    Top = 5,
                    SuggesterName = "test"
                });
                Assert.IsTrue(results.Any(x => x.Text.Equals("location")));
            }
        }
    }
}
