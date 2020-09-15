using Examine.LuceneEngine.Providers;
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

namespace Examine.Test.Extensions
{
    [TestFixture]
    public class SpatialSearch
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
            var strategy = GetRecursivePrefixTreeStrategy(ctx);
            RunTest(ctx, strategy, a => new MatchAllDocsQuery());
        }

        [Test]
        public void Document_Writing_To_Index_Spatial_Data_And_Search_On_100km_Radius_GetPointVectorStrategy()
        {
            SpatialContext ctx = SpatialContext.GEO;
            var strategy = GetPointVectorStrategy(ctx);
            // NOTE: This works without this custom query and only using the filter too
            // there's also almost zero documentation (even in java) on what MakeQueryDistanceScore actually does, 
            // the source is here https://lucenenet.apache.org/docs/3.0.3/d0/d37/_point_vector_strategy_8cs_source.html#l00133
            RunTest(ctx, strategy, a => strategy.MakeQueryDistanceScore(a));
        }

        private void RunTest(SpatialContext ctx, SpatialStrategy strategy, Func<SpatialArgs, Query> createQuery)
        {
            var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = new TestIndex(luceneDir, analyzer))
            {
                indexer.DocumentWriting += (sender, args) => Indexer_DocumentWriting(args, ctx, strategy);

                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "location 1", bodyText = "Zanzibar is in Africa", lat = -6.1357, lng = 39.3621}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "location 2", bodyText = "In Canada there is a town called Sydney in Nova Scotia", lat = 46.1368, lng = -60.1942 }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "location 3", bodyText = "Sydney is the capital of NSW in Australia", lat = -33.8688, lng = 151.2093 })
                    });

                DoSpatialSearch(ctx, strategy, indexer, SearchRadius, lat: -33, lng: 151, "3", createQuery);
                DoSpatialSearch(ctx, strategy, indexer, SearchRadius, lat: 46, lng: -60, "2", createQuery);
                DoSpatialSearch(ctx, strategy, indexer, SearchRadius, lat: -6, lng: 39, "1", createQuery);
            }
        }

        private void DoSpatialSearch(
            SpatialContext ctx, SpatialStrategy strategy, 
            TestIndex indexer, double searchRadius, int lat, int lng, string idToMatch,
            Func<SpatialArgs, Query> createQuery)
        {
            GetXYFromCoords(lat, lng, out var x, out var y);

            // Make a circle around the search point
            var args = new SpatialArgs(
                SpatialOperation.Intersects,
                ctx.MakeCircle(x, y, DistanceUtils.Dist2Degrees(searchRadius, DistanceUtils.EARTH_MEAN_RADIUS_KM)));

            var filter = strategy.MakeFilter(args);

            var searcher = (LuceneSearcher)indexer.GetSearcher();
            var luceneSearcher = searcher.GetLuceneSearcher();

            var query = createQuery(args);

            // TODO: It doesn't make a whole lot of sense to sort by score when searching on only geo-coords, 
            // typically you would sort by closest distance
            // Which can be done, see https://github.com/apache/lucene-solr/blob/branch_4x/lucene/spatial/src/test/org/apache/lucene/spatial/SpatialExample.java#L169
            TopDocs docs = luceneSearcher.Search(query, filter, MaxResultDocs, new Sort(new SortField(null, SortField.SCORE)));

            AssertDocMatchedIds(luceneSearcher, docs, idToMatch);


            // TODO: We should make this possible and allow passing in a Lucene Filter
            // to the LuceneSearchQuery along with the Lucene Query, then we
            // don't need to manually perform the Lucene Search

            //var criteria = (LuceneSearchQuery)searcher.CreateQuery();
            //criteria.LuceneQuery(q);
            //var results = criteria.Execute();
        }

        private void AssertDocMatchedIds(Searcher indexSearcher, TopDocs docs, string idToMatch)
        {            
            string[] gotIds = new string[docs.TotalHits];
            for (int i = 0; i < gotIds.Length; i++)
            {
                var doc = indexSearcher.Doc(docs.ScoreDocs[i].Doc);
                var id = doc.GetField(LuceneIndex.ItemIdFieldName).StringValue;
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

        private void Indexer_DocumentWriting(LuceneEngine.DocumentWritingEventArgs e, SpatialContext ctx, SpatialStrategy strategy)
        {
            double lat = double.Parse(e.ValueSet.Values["lat"].First().ToString());
            double lng = double.Parse(e.ValueSet.Values["lng"].First().ToString());

            GetXYFromCoords(lat, lng, out var x, out var y);
            Shape geoPoint = ctx.MakePoint(x, y);

            foreach (AbstractField field in strategy.CreateIndexableFields(geoPoint))
            {
                e.Document.Add(field);
            }
        }

        private RecursivePrefixTreeStrategy GetRecursivePrefixTreeStrategy(SpatialContext ctx)
        {
            int maxLevels = 11; //results in sub-meter precision for geohash
            SpatialPrefixTree grid = new GeohashPrefixTree(ctx, maxLevels);
            RecursivePrefixTreeStrategy strategy2 = new RecursivePrefixTreeStrategy(grid, GeoLocationFieldName);            
            return strategy2;
        }

        private PointVectorStrategy GetPointVectorStrategy(SpatialContext ctx)
        {
            PointVectorStrategy strategy = new PointVectorStrategy(ctx, GeoLocationFieldName);
            return strategy;
        }
    }
}
