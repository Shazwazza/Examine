using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Examine.Test.DataServices;
using Examine.Test.PartialTrust;
using Examine.Test.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using UmbracoExamine;

namespace Examine.Test.Index
{
    [TestFixture]
	public class EventsTest : AbstractPartialTrustFixture<EventsTest>
    {
        [Test]
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
		private Lucene.Net.Store.Directory _luceneDir;

		public override void TestSetup()
        {
			_luceneDir = new RAMDirectory();
			_indexer = IndexInitializer.GetUmbracoIndexer(_luceneDir);
            _indexer.RebuildIndex();
			_searcher = IndexInitializer.GetUmbracoSearcher(_luceneDir);
        }

		public override void TestTearDown()
		{
			_luceneDir.Dispose();
		}
    }
}
