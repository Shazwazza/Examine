using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Search;
using Lucene.Net.Analysis.Standard;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene.Search
{
    [TestFixture]
    public class ExamineValueTests : ExamineBaseTest
    {
        [TestCase(false)]
        [TestCase(true)]
        public void TODO_THIS_ALSO_NEEDS_A_NAME(bool examineValueAsInterface)
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var luceneTaxonomyDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                       luceneDir,
                       luceneTaxonomyDir,
                       analyzer))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "cOntent",
                        new { someField = new [] { "lorem", "ipsum" } }),
                    ValueSet.FromObject(2.ToString(), "cOntent",
                        new { someField = new [] { "lorem", "ipsum" } })
                });

                var searcher = indexer.Searcher;
                var query = examineValueAsInterface
                    ? searcher
                        .CreateQuery("cOntent")
                        .Field("someField", (IExamineValue)new ExamineValue(Examineness.Explicit, "lorem"))
                    : searcher
                        .CreateQuery("cOntent")
                        .Field("someField", new ExamineValue(Examineness.Explicit, "lorem"));

                var results = query.Execute();
                Assert.AreEqual(2, results.TotalItemCount);
            }
        }
    }
}
