using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UmbracoExamine;
using System.Diagnostics;
using UmbracoExamine.PDF;
using System.Xml.Linq;
using Examine.Test.DataServices;

namespace Examine.Test
{
    [TestClass]
    public class PDFIndexerTests
    {
        #region Private methods

        /// <summary>
        /// Helper method to return the index searcher for this index
        /// </summary>
        /// <returns></returns>
        private UmbracoExamineSearcher GetSearcherProvider()
        {
            return (UmbracoExamineSearcher)ExamineManager.Instance.SearchProviderCollection["PDFSearcher"];
        }

        /// <summary>
        /// Helper method to return the indexer that we are testing
        /// </summary>
        /// <returns></returns>
        private PDFIndexer GetIndexer()
        {
            return (PDFIndexer)ExamineManager.Instance.IndexProviderCollection["PDFIndexer"];
        }
        #endregion

        private static IndexInitializer m_Init;
        private TestMediaService m_MediaService = new TestMediaService();

        [TestInitialize()]
        public void Initialize()
        {
            m_Init = new IndexInitializer();
            GetIndexer().RebuildIndex();
        }

        [TestMethod]
        public void PDFIndexer_Ensure_ParentID_Honored()
        {
            var indexer = GetIndexer();
            //change parent id to 1116
            ((IndexCriteria)indexer.IndexerData).ParentNodeId = 1116;

            //get the 2112 pdf node: 2112
            var node = m_MediaService.GetLatestMediaByXpath("//*[string-length(@id)>0 and number(@id)>0]")
                .Root
                .Elements()
                .Where(x => (int)x.Attribute("id") == 2112)
                .First();

            //create a copy of 2112 undneath 1111 which is 'not indexable'
            var newpdf = XElement.Parse(node.ToString());
            newpdf.SetAttributeValue("id", "999999");
            newpdf.SetAttributeValue("path", "-1,1111,999999");
            newpdf.SetAttributeValue("parentID", "1111");

            //now reindex
            indexer.ReIndexNode(newpdf, IndexTypes.Media);

            //make sure it doesn't exist
            var search = GetSearcherProvider();
            var results = search.Search(search.CreateSearchCriteria().Id(999999).Compile());
            Assert.AreEqual<int>(0, results.Count());
        }

        [TestMethod]
        public void PDFIndexer_Reindex()
        {
            var indexer = GetIndexer();

            //get searcher and reader to get stats
            var s = GetSearcherProvider();
            var r = s.GetSearcher().GetIndexReader();

            Trace.Write("Num docs = " + r.NumDocs().ToString());

            Assert.AreEqual<int>(7, r.NumDocs());

            //search the pdf content to ensure it's there
            Assert.IsTrue(s.Search(s.CreateSearchCriteria().Id(1113).Compile()).Single()
                .Fields[PDFIndexer.TextContentFieldName].Contains("EncapsulateField"));
            Assert.IsTrue(s.Search(s.CreateSearchCriteria().Id(1114).Compile()).Single()
                .Fields[PDFIndexer.TextContentFieldName].Contains("metaphysical realism"));

            //the contour PDF cannot be read properly, this is to due with the PDF encoding!
            //Assert.IsTrue(s.Search(s.CreateSearchCriteria().Id(1115).Compile()).Single()
            //    .Fields[PDFIndexer.TextContentFieldName].Contains("Returns All records from the form with the id"));

            Assert.IsTrue(s.Search(s.CreateSearchCriteria().Id(1116).Compile()).Single()
                .Fields[PDFIndexer.TextContentFieldName].Contains("What long-term preservation"));
            

        }
    }
}
