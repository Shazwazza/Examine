using Examine.Lucene;
using Examine.Lucene.Providers;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Spatial;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Spatial.Queries;
using Lucene.Net.Spatial.Vector;
using NUnit.Framework;
using Spatial4n.Core.Context;
using Spatial4n.Core.Distance;
using Spatial4n.Core.Shapes;
using System;
using System.Linq;

namespace Examine.Test.Examine.Lucene.Extensions
{
    [TestFixture]
    public class SpatialSearch : ExamineBaseTest
    {
        private const string GeoLocationFieldName = "geoLocation";
        private const int MaxResultDocs = 10;
        private const double SearchRadius = 100; // in KM

        [Test]
        public void Document_Writing_To_Index_Spatial_Data_And_Search_On_100km_Radius_RecursivePrefixTreeStrategy()
        {
            // NOTE: It is advised to use RecursivePrefixTreeStrategy, see: 
            // https://stackoverflow.com/a/13631289/694494
            // Here's the Java sample code 
            // https://github.com/apache/lucene-solr/blob/branch_4x/lucene/spatial/src/test/org/apache/lucene/spatial/SpatialExample.java

            SpatialContext ctx = SpatialContext.GEO;
            int maxLevels = 11; //results in sub-meter precision for geohash
            SpatialPrefixTree grid = new GeohashPrefixTree(ctx, maxLevels);
            var strategy = new RecursivePrefixTreeStrategy(grid, GeoLocationFieldName);

            // NOTE: The SpatialExample uses MatchAllDocsQuery however the strategy can create a query too, the source is here:
            // https://github.com/apache/lucenenet/blob/master/src/Lucene.Net.Spatial/SpatialStrategy.cs#L124
            // all that really does it take the filter created and creates a ConstantScoreQuery with it
            // which probably makes sense because a 'Score' for a geo coord doesn't make a lot of sense.
            RunTest(ctx, strategy, a => strategy.MakeQuery(a));
        }

        [Test]
        public void Document_Writing_To_Index_Spatial_Data_And_Search_On_100km_Radius_GetPointVectorStrategy()
        {
            SpatialContext ctx = SpatialContext.GEO;
            var strategy = new PointVectorStrategy(ctx, GeoLocationFieldName);

            // NOTE: This works without this custom query and only using the filter too
            // there's also almost zero documentation (even in java) on what MakeQueryDistanceScore actually does, 
            // the source is here https://lucenenet.apache.org/docs/3.0.3/d0/d37/_point_vector_strategy_8cs_source.html#l00133
            // And as it's noted it shouldn't be used: "this is basically old code that hasn't been verified well and should probably be removed"
            // It's also good to note that PointVectorStrategy is 'obsolete' as it exists now under the Legacy namespace in Java Lucene

            // NOTE: The SpatialExample uses MatchAllDocsQuery however the strategy can create a query too, the source is here:
            // https://lucenenet.apache.org/docs/3.0.3/d0/d37/_point_vector_strategy_8cs_source.html#l00104
            // which looks like it verifies that the search is using an Intersects/Within query and then creates a 
            // ConstantScoreQuery - which probably makes sense because a 'Score' for a geo coord doesn't make a lot of sense.
            RunTest(ctx, strategy, a => strategy.MakeQuery(a));
        }

        private void RunTest(SpatialContext ctx, SpatialStrategy strategy, Func<SpatialArgs, Query> createQuery)
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            {
                string id1 = 1.ToString();
                string id2 = 2.ToString();
                string id3 = 3.ToString();
                string id4 = 4.ToString();

                using (var indexer = GetTestIndex(luceneDir, analyzer))
                {
                    indexer.DocumentWriting += (sender, args) => Indexer_DocumentWriting(args, ctx, strategy);

                    indexer.IndexItems(new[] {
                    ValueSet.FromObject(id1, "content",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa", lat = -6.1357, lng = 39.3621}),
                    ValueSet.FromObject(id2, "content",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia", lat = 46.1368, lng = -60.1942 }),
                    ValueSet.FromObject(id3, "content",
                        new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia", lat = -33.8688, lng = 151.2093 }),
                    ValueSet.FromObject(id4, "content",
                        new { nodeName = "location 4", bodyText = "Somewhere unknown", lat = 50, lng = 50 })
                    });

                    DoSpatialSearch(ctx, strategy, indexer, SearchRadius, id3, createQuery, lat: -33, lng: 151);
                    DoSpatialSearch(ctx, strategy, indexer, SearchRadius, id2, createQuery, lat: 46, lng: -60);
                    DoSpatialSearch(ctx, strategy, indexer, SearchRadius, id1, createQuery, lat: -6, lng: 39);
                    DoSpatialSearch(ctx, strategy, indexer, SearchRadius, id4, createQuery, lat: 50, lng: 50);
                }
            }

        }

        private void DoSpatialSearch(
            SpatialContext ctx, SpatialStrategy strategy,
            TestIndex indexer, double searchRadius, string idToMatch, Func<SpatialArgs, Query> createQuery, int lat,
            int lng)
        {
            var searcher = (LuceneSearcher)indexer.Searcher;
            var searchContext = searcher.GetSearchContext();

            using (var searchRef = searchContext.GetSearcher())
            {
                GetXYFromCoords(lat, lng, out var x, out var y);

                // Make a circle around the search point
                var args = new SpatialArgs(
                    SpatialOperation.Intersects,
                    ctx.MakeCircle(x, y, DistanceUtils.Dist2Degrees(searchRadius, DistanceUtils.EARTH_MEAN_RADIUS_KM)));

                var filter = strategy.MakeFilter(args);

                var query = createQuery(args);

                // TODO: It doesn't make a whole lot of sense to sort by score when searching on only geo-coords, 
                // typically you would sort by closest distance
                // Which can be done, see https://github.com/apache/lucene-solr/blob/branch_4x/lucene/spatial/src/test/org/apache/lucene/spatial/SpatialExample.java#L169
                TopDocs docs = searchRef.IndexSearcher.Search(query, filter, MaxResultDocs, new Sort(new SortField(null, SortFieldType.SCORE)));

                AssertDocMatchedIds(searchRef.IndexSearcher, docs, idToMatch);


                // TODO: We should make this possible and allow passing in a Lucene Filter
                // to the LuceneSearchQuery along with the Lucene Query, then we
                // don't need to manually perform the Lucene Search

                //var criteria = (LuceneSearchQuery)searcher.CreateQuery();
                //criteria.LuceneQuery(q);
                //var results = criteria.Execute(); 
            }
        }

        private void AssertDocMatchedIds(IndexSearcher indexSearcher, TopDocs docs, string idToMatch)
        {
            string[] gotIds = new string[docs.TotalHits];
            for (int i = 0; i < gotIds.Length; i++)
            {
                var doc = indexSearcher.Doc(docs.ScoreDocs[i].Doc);
                var id = doc.GetField(ExamineFieldNames.ItemIdFieldName).GetStringValue();
                gotIds[i] = id;
            }
            Assert.AreEqual(1, gotIds.Length);
            Assert.AreEqual(idToMatch, gotIds[0]);
        }

        private void GetXYFromCoords(double lat, double lng, out double x, out double y)
        {
            // Important! we need to change to x/y coords, longitude = x, latitude = y
            x = lng;
            y = lat;
        }

        private void Indexer_DocumentWriting(DocumentWritingEventArgs e, SpatialContext ctx, SpatialStrategy strategy)
        {
            double lat = double.Parse(e.ValueSet.Values["lat"].First().ToString());
            double lng = double.Parse(e.ValueSet.Values["lng"].First().ToString());

            GetXYFromCoords(lat, lng, out var x, out var y);
            IPoint geoPoint = ctx.MakePoint(x, y);

            foreach (Field field in strategy.CreateIndexableFields(geoPoint))
            {
                e.Document.Add(field);
            }
        }
    }
}
