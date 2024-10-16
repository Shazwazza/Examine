using Lucene.Net.Analysis;
using Lucene.Net.Store;

namespace Examine.Benchmarks
{
    internal class InitTools
    {
        public static TestIndex InitializeIndex(
            ExamineBaseTest examineBaseTest,
            string tempBasePath,
            Analyzer analyzer,
            out DirectoryInfo indexDir)
        {
            var tempPath = Path.Combine(tempBasePath, Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(tempPath);
            indexDir = new DirectoryInfo(tempPath);
            var luceneDirectory = FSDirectory.Open(indexDir);
            var indexer = examineBaseTest.GetTestIndex(luceneDirectory, analyzer);
            return indexer;
        }

        public static List<ValueSet> CreateValueSet(int count)
        {
            var random = new Random();
            var valueSets = new List<ValueSet>();

            for (var i = 0; i < count; i++)
            {
                valueSets.Add(ValueSet.FromObject(Guid.NewGuid().ToString(), "content",
                    new
                    {
                        nodeName = "location " + i,
                        bodyText = Enumerable.Range(0, random.Next(10, 100)).Select(x => Guid.NewGuid().ToString())
                    }));
            }

            return valueSets;
        }
    }
}
