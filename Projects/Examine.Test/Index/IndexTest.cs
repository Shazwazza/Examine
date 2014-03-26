using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Examine.LuceneEngine.Indexing;
using Examine.Session;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Moq;
using NUnit.Framework;

using Lucene.Net.Search;
using Lucene.Net.Index;
using System.Diagnostics;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using System.Threading;
using Examine.Test.DataServices;
using UmbracoExamine;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace Examine.Test.Index
{
    [TestFixture]
    public class LuceneIndexerTests
    {
        private class TestIndexer : LuceneIndexer
        {
            public TestIndexer(IIndexCriteria indexerData, Directory luceneDirectory, Analyzer analyzer) 
                : base(indexerData, luceneDirectory, analyzer)
            {
            }

            protected override void PerformIndexAll(string type)
            {
                throw new NotImplementedException();
            }

            protected override void PerformIndexRebuild()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void Can_Add_One_Document()
        {
            var luceneDir = new RAMDirectory();
            var indexer = new TestIndexer(
                Mock.Of<IIndexCriteria>(),
                luceneDir,
                new StandardAnalyzer(Version.LUCENE_29));

            indexer.AddDocument(new ValueSet(1, "test", "content",
                new Dictionary<string, List<object>>
                {
                    {"item1", new List<object>(new[] {"value1"})},
                    {"item2", new List<object>(new[] {"value2"})}
                }));

            ExamineSession.WaitForChanges();

            var sc = indexer.SearcherContext;
            using (var s = sc.GetSearcher())
            {
                Assert.AreEqual(1, s.Searcher.GetIndexReader().NumDocs());
            }
        }

        [Test]
        public void Can_Add_Same_Document_Twice_Without_Duplication()
        {
            var luceneDir = new RAMDirectory();
            var indexer = new TestIndexer(
                Mock.Of<IIndexCriteria>(),
                luceneDir,
                new StandardAnalyzer(Version.LUCENE_29));

            var value = new ValueSet(1, "test", "content",
                new Dictionary<string, List<object>>
                {
                    {"item1", new List<object>(new[] {"value1"})},
                    {"item2", new List<object>(new[] {"value2"})}
                });

            indexer.AddDocument(value);
            ExamineSession.WaitForChanges();

            indexer.AddDocument(value);
            ExamineSession.WaitForChanges();

            var sc = indexer.SearcherContext;
            using (var s = sc.GetSearcher())
            {
                Assert.AreEqual(1, s.Searcher.GetIndexReader().NumDocs());
            }
        }

        [Test]
        public void Can_Add_Multiple_Docs()
        {
            var luceneDir = new RAMDirectory();
            var indexer = new TestIndexer(
                Mock.Of<IIndexCriteria>(),
                luceneDir,
                new StandardAnalyzer(Version.LUCENE_29));
            
            for (var i = 0; i < 10; i++)
            {
                indexer.AddDocument(new ValueSet(i, "test", "content",
                    new Dictionary<string, List<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new[] {"value2"})}
                    }));                
            }

            ExamineSession.WaitForChanges();

            var sc = indexer.SearcherContext;
            using (var s = sc.GetSearcher())
            {
                Assert.AreEqual(10, s.Searcher.GetIndexReader().NumDocs());
            }
        }

        [Test]
        public void Can_Delete()
        {
            var luceneDir = new RAMDirectory();
            var indexer = new TestIndexer(
                Mock.Of<IIndexCriteria>(),
                luceneDir,
                new StandardAnalyzer(Version.LUCENE_29));

            for (var i = 0; i < 10; i++)
            {
                indexer.AddDocument(new ValueSet(i, "test", "content",
                    new Dictionary<string, List<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new[] {"value2"})}
                    }));
            }
            indexer.DeleteFromIndex("9");

            ExamineSession.WaitForChanges();

            var sc = indexer.SearcherContext;
            using (var s = sc.GetSearcher())
            {
                Assert.AreEqual(9, s.Searcher.GetIndexReader().NumDocs());
            }
        }

    }

    /// <summary>
    /// Tests the standard indexing capabilities
    /// </summary>
    [TestFixture, RequiresSTA]
    public class IndexTest 
    {

      
        [Test]
        public void Index_Ensure_No_Duplicates_In_Non_Async()
        {
            //get a node from the data repo
            var node = _contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                .Root
                .Elements()
                .First();

            //get the id for th node we're re-indexing.
            var id = (int)node.Attribute("id");

            //reindex the same node a bunch of times
            for (var i = 0; i < 29; i++)
            {
                _indexer.ReIndexNode(node, IndexTypes.Content);
            }

            //ensure no duplicates
            var results = _searcher.Search(_searcher.CreateSearchCriteria().Id(id).Compile());
            Assert.AreEqual(1, results.Count());
        }
        
        [Test]
        public void Index_Rebuild_Index()
        {

            //get searcher and reader to get stats
            var r = ((IndexSearcher)_searcher.GetSearcher()).GetIndexReader();

            //there's 16 fields in the index, but 3 sorted fields
            var fields = r.GetFieldNames(IndexReader.FieldOption.ALL);

            Assert.AreEqual(21, fields.Count());
            //ensure there's 3 sorting fields
            Assert.AreEqual(4, fields.Count(x => x.StartsWith(LuceneIndexer.SortedFieldNamePrefix)));
            //there should be 11 documents (10 content, 1 media)
            Assert.AreEqual(10, r.NumDocs());

            //test for the special fields to ensure they are there:
            Assert.AreEqual(1, fields.Count(x => x == LuceneIndexer.IndexNodeIdFieldName));
            Assert.AreEqual(1, fields.Count(x => x == LuceneIndexer.IndexTypeFieldName));
            Assert.AreEqual(1, fields.Count(x => x == UmbracoContentIndexer.IndexPathFieldName));
            Assert.AreEqual(1, fields.Count(x => x == UmbracoContentIndexer.NodeTypeAliasFieldName));

        }



        #region Private methods and properties

        private readonly TestContentService _contentService = new TestContentService();
        private readonly TestMediaService _mediaService = new TestMediaService();

        private static UmbracoExamineSearcher _searcher;
        private static UmbracoContentIndexer _indexer;

        #endregion

        #region Initialize and Cleanup

        private Lucene.Net.Store.Directory _luceneDir;

        [TearDown]
        public void TestTearDown()
        {
            _luceneDir.Dispose();
        }

        [SetUp]
        public void TestSetup()
        {
            _luceneDir = new RAMDirectory();
            _indexer = IndexInitializer.GetUmbracoIndexer(_luceneDir);
            _indexer.RebuildIndex();
            _searcher = IndexInitializer.GetUmbracoSearcher(_luceneDir);
        }


        #endregion
    }
}
