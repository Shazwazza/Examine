using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Lucene;
using Examine.Lucene.Indexing;
using Examine.Lucene.Spatial;
using Examine.Search;
using Lucene.Net.Analysis.Standard;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene.Search
{
    [TestFixture]
    public partial class FluentApiTests : ExamineBaseTest
    {
        [Test]
        public void Sort_Result_By_Geo_Spatial_Field_Distance()
        {
            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            var examineDefault = ValueTypeFactoryCollection.GetDefaultValueTypes(Logging, analyzer);
            var examineSpatialDefault = SpatialValueTypeFactoryCollection.GetDefaultValueTypes(Logging, analyzer);
            Dictionary<string, IFieldValueTypeFactory> valueTypeFactoryDictionary = new Dictionary<string, IFieldValueTypeFactory>(examineDefault);
            foreach (var item in examineSpatialDefault)
            {
                valueTypeFactoryDictionary.Add(item.Key, item.Value);
            }

            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                //Ensure it's set to a date, otherwise it's not sortable
                new FieldDefinitionCollection(
                    new FieldDefinition("updateDate", FieldDefinitionTypes.DateTime),
                    new FieldDefinition("parentID", FieldDefinitionTypes.Integer),
                    new FieldDefinition("spatialWKT", FieldDefinitionTypes.GeoSpatialWKT)
                ), indexValueTypesFactory: valueTypeFactoryDictionary))
            {
                var now = DateTime.Now;
                var geoSpatialFieldType = indexer.FieldValueTypeCollection.ValueTypes.First(f
                    => f.FieldName.Equals("spatialWKT", StringComparison.InvariantCultureIgnoreCase)) as ISpatialIndexFieldValueTypeBase;

                var fieldShapeFactory = geoSpatialFieldType.ExamineSpatialShapeFactory;

                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { nodeName = "my name 1", updateDate = now.AddDays(2).ToString("yyyy-MM-dd"), parentID = "1143" , spatialWKT = fieldShapeFactory.CreatePoint(0.0,0.0) }),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { nodeName = "my name 2", updateDate = now.ToString("yyyy-MM-dd"), parentID = 1143, spatialWKT = fieldShapeFactory.CreatePoint(1.0,1.0) }),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { nodeName = "my name 3", updateDate = now.AddDays(1).ToString("yyyy-MM-dd"), parentID = 1143, spatialWKT = fieldShapeFactory.CreatePoint(2.0,2.0) }),
                    ValueSet.FromObject(4.ToString(), "content",
                        new { nodeName = "my name 4", updateDate = now, parentID = "2222", spatialWKT = fieldShapeFactory.CreatePoint(3.0,3.0) }),
                    });

                var searcher = indexer.Searcher;
                var searchLocation = fieldShapeFactory.CreatePoint(0.0, 0.0);
                var sc = searcher.CreateQuery("content");
                var sorting = new Sorting[]
                {
                    new Sorting(new SortableField("spatialWKT",searchLocation),SortDirection.Ascending),
                    //new Sorting(new SortableField("updateDate", SortType.Long), SortDirection.Ascending)
                };
                var sc1 = sc.Field("parentID", 1143)
                    .OrderBy(sorting);

                var results1 = sc1.Execute().ToArray();

                Assert.AreEqual(3, results1.Length);

            }
        }
    }
}
