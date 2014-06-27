using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Examine.Session;
using Examine.Test.DataServices;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using UmbracoExamine;
using Version = Lucene.Net.Util.Version;

namespace Examine.Test.Search
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestFixture]
	public class SearchTest
    {

        [TearDown]
        public void Teardown()
        {
            DisposableCollector.Clean();
        }

        [Test]
        public void Search_All_Fields_No_Wildcards()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            {
                indexer.RebuildIndex();
                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);
                var result = searcher.Search("value1", false);

                Assert.AreEqual(1, result.TotalItemCount);
            }    
        }

        [Test]
        public void Search_All_Fields_Wildcards()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            {
                indexer.RebuildIndex();
                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);
                var result = searcher.Search("value", true);

                Assert.AreEqual(100, result.TotalItemCount);
            }
        }

        [Test]
        public void Search_On_Stop_Word_No_Result()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            {
                indexer.IndexItems(new ValueSet(1, "content",
                   new { item1 = "value1", item2 = "here we go into the darkness" }));

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);
                var result = searcher.Search("into", false);

                Assert.AreEqual(0, result.TotalItemCount);
            }
        }

    }
}
