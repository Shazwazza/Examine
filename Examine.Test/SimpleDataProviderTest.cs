using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Examine.Test.DataServices;
using Examine.LuceneEngine;

namespace Examine.Test
{
    [TestClass]
    public class SimpleDataProviderTest
    {
        [TestMethod]
        public void SimpleData_RebuildIndex()
        {
            var indexer = ExamineManager.Instance.IndexProviderCollection["SimpleIndexer"];            

            indexer.RebuildIndex();
            
            //now, we need to ensure the right data is in there....
            
            //get searcher and reader to get stats
            var s = (LuceneSearcher)ExamineManager.Instance.SearchProviderCollection["SimpleSearcher"];
            var r = s.GetSearcher().GetIndexReader();

            //there's 7 fields in the index, but 1 sorted fields, 2 are special fields
            var fields = r.GetFieldNames(IndexReader.FieldOption.ALL);

            Assert.AreEqual(9, fields.Count());
            
            //there should be 5 documents (2 Documents, 3 Pictures)
            Assert.AreEqual(5, r.NumDocs());

            //test for the special fields to ensure they are there:
            Assert.AreEqual<int>(1, fields.Where(x => x == LuceneIndexer.IndexNodeIdFieldName).Count());
            Assert.AreEqual<int>(1, fields.Where(x => x == LuceneIndexer.IndexTypeFieldName).Count());

        }

        [TestMethod]
        public void SimpleData_Reindex_Node()
        {
            //first, we'll rebuild test to make sure we have the correct data in there

            SimpleData_RebuildIndex();

            //now we'll index one new node:

            var indexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["SimpleIndexer"];

            var dataSet =  ((TestSimpleDataProvider)indexer.DataService).CreateNewDocument();
            var xml = dataSet.RowData.ToExamineXml(dataSet.NodeDefinition.NodeId, dataSet.NodeDefinition.Type);

            indexer.ReIndexNode(xml, "Documents");

            //get searcher and reader to get stats
            var s = (LuceneSearcher)ExamineManager.Instance.SearchProviderCollection["SimpleSearcher"];
            var r = s.GetSearcher().GetIndexReader();            

            //there should be 6 documents now (3 Documents, 3 Pictures)
            Assert.AreEqual(6, r.NumDocs());        
                        
        }

        [TestMethod]
        public void SimpleDataProviderTest_Range_Search_On_Year()
        {
            //Arrange
            SimpleData_RebuildIndex();
            var s = (LuceneSearcher)ExamineManager.Instance.SearchProviderCollection["SimpleSearcher"];

            //Act
            var query = s.CreateSearchCriteria().Range("YearCreated", DateTime.Now.AddYears(-1), DateTime.Now, true, true, SearchCriteria.DateResolution.Year).Compile();
            var results = s.Search(query);

            //Assert
            Assert.AreEqual(5, results.TotalItemCount);
        }
    }
}
