using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Lucene.Net.Search;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Examine.Test.DataServices;
using Examine.LuceneEngine;
using Lucene.Net.Store;
using NUnit.Framework;

namespace Examine.Test
{
    [TestFixture]
	public class SimpleDataProviderTest
    {
        [Test]
        public void SimpleData_RebuildIndex()
        {    
                        
            //now, we need to ensure the right data is in there....
            
            //get searcher and reader to get stats
            var r = ((IndexSearcher)_searcher.GetLuceneSearcher()).IndexReader;

            //there's 7 fields in the index, but 1 sorted fields, 2 are special fields
            var fields = r.GetFieldNames(IndexReader.FieldOption.ALL);

            Assert.AreEqual(16, fields.Count());
            
            //there should be 5 documents (2 Documents, 3 Pictures)
            Assert.AreEqual(5, r.NumDocs());

            //test for the special fields to ensure they are there:
            Assert.AreEqual(1, fields.Count(x => x == LuceneIndexer.NodeIdFieldName));
            Assert.AreEqual(1, fields.Count(x => x == LuceneIndexer.CategoryFieldName));

        }

        [Test]
        public void SimpleData_Reindex_Node()
        {

            //now we'll index one new node:

            var dataSet =  ((TestValueSetDataProvider)_indexer.DataService).CreateNewDocument(DateTime.Now);

            _indexer.IndexItems(new[] {dataSet});

            //get searcher and reader to get stats            
            var r = ((IndexSearcher)_searcher.GetLuceneSearcher()).IndexReader;      

            //there should be 6 documents now (3 Documents, 3 Pictures)
            Assert.AreEqual(6, r.NumDocs());        
                        
        }

        //[Test]
        //public void SimpleDataProviderTest_Range_Search_On_Year()
        //{
        //    //Act
        //    var query = _searcher.CreateSearchCriteria().Range("YearCreated", DateTime.Now.AddYears(-1), DateTime.Now, true, true, SearchCriteria.DateResolution.Year).Compile();
        //    var results = _searcher.Search(query);

        //    //Assert
        //    Assert.AreEqual(5, results.TotalItemCount);
        //}

        private static SimpleDataIndexer _indexer;
        private static LuceneSearcher _searcher;
		private Lucene.Net.Store.Directory _luceneDir;

        [TestFixtureSetUp]
        public void TestSetup()
		{
			_luceneDir = new RandomIdRAMDirectory();
			_indexer = IndexInitializer.GetSimpleIndexer(_luceneDir);
			_indexer.RebuildIndex();
		    _searcher = (LuceneSearcher)_indexer.GetSearcher();

		}

        [TestFixtureTearDown]
        public void TestTearDown()
		{
			_luceneDir.Dispose();
		}
    }
}
