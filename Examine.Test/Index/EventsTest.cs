using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Examine.Test.DataServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UmbracoExamine;

namespace Examine.Test.Index
{
    [TestClass]
    public class EventsTest
    {
        [TestMethod]
        public void Events_Ignoring_Node()
        {
            //change the parent id so that they are all ignored
            ((IndexCriteria)_indexer.IndexerData).ParentNodeId = 999;

            var isIgnored = false;

            EventHandler<IndexingNodeDataEventArgs> ignoringNode = (s,e) =>
            {
                isIgnored = true;
            };

            _indexer.IgnoringNode += ignoringNode;

            //get a node from the data repo
            var node = _contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                .Root
                .Elements()
                .First();

            _indexer.ReIndexNode(node, IndexTypes.Content);


            Assert.IsTrue(isIgnored);

        }

        private readonly TestContentService _contentService = new TestContentService();
        private static UmbracoExamineSearcher _searcher;
        private static UmbracoContentIndexer _indexer;

        [ClassInitialize()]
        public static void Initialize(TestContext context)
        {
            var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\EventsTest", Guid.NewGuid().ToString()));
            _indexer = IndexInitializer.GetUmbracoIndexer(newIndexFolder);
            _indexer.RebuildIndex();
            _searcher = IndexInitializer.GetUmbracoSearcher(newIndexFolder);
        }
    }
}
