using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Indexing.ValueTypes;
using Examine.LuceneEngine.Providers;
using Examine.Session;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Moq;
using NUnit.Framework;
using UmbracoExamine;
using Version = Lucene.Net.Util.Version;

namespace Examine.Test.Index
{
    [TestFixture]
    public class LuceneIndexerTests
    {

        [Test]
        public void Rebuild_Index()
        {
            using (var luceneDir = new RAMDirectory())            
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_29)))
            {
                indexer.RebuildIndex();

                ExamineSession.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    Assert.AreEqual(100, s.Searcher.GetIndexReader().NumDocs());
                }
            }
        }

        [Test]
        public void Reindex_Item_Type()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_29)))
            {

                indexer.IndexAll("category0");

                ExamineSession.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    Assert.AreEqual(50, s.Searcher.GetIndexReader().NumDocs());
                }

                indexer.IndexAll("category1");

                ExamineSession.WaitForChanges();

                sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    Assert.AreEqual(100, s.Searcher.GetIndexReader().NumDocs());
                }
            }
        }

        [Test]
        public void Index_Exists()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_29)))
            {

                Assert.IsTrue(indexer.IndexExists());
            }
        }

        [Test]
        public void Can_Add_One_Document()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_29)))
            {

                indexer.IndexItems(new ValueSet(1, "content",
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
        }

        [Test]
        public void Can_Add_Same_Document_Twice_Without_Duplication()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_29)))
            {

                var value = new ValueSet(1, "content",
                    new Dictionary<string, List<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new[] {"value2"})}
                    });

                indexer.IndexItems(value);
                ExamineSession.WaitForChanges();

                indexer.IndexItems(value);
                ExamineSession.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    Assert.AreEqual(1, s.Searcher.GetIndexReader().NumDocs());
                }
            }
        }

        [Test]
        public void Can_Add_Multiple_Docs()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_29)))
            {

                for (var i = 0; i < 10; i++)
                {
                    indexer.IndexItems(new ValueSet(i, "content",
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
        }

        [Test]
        public void Can_Delete()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_29)))
            {

                for (var i = 0; i < 10; i++)
                {
                    indexer.IndexItems(new ValueSet(i, "content",
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

        [Test]
        public void Can_Add_Doc_With_Fields()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_29)))
            {

                indexer.IndexItems(new ValueSet(1, "content",
                    new Dictionary<string, List<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new[] {"value2"})}
                    }));

                ExamineSession.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    var fields = s.Searcher.Doc(0).GetFields().Cast<Fieldable>().ToArray();
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name() == "item1"));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name() == "item2"));
                    Assert.AreEqual("value1", fields.Single(x => x.Name() == "item1").StringValue());
                    Assert.AreEqual("value2", fields.Single(x => x.Name() == "item2").StringValue());
                }
            }
        }

        [Test]
        public void Can_Add_Doc_With_Easy_Fields()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_29)))
            {

                indexer.IndexItems(new ValueSet(1, "content",
                    new {item1 = "value1", item2 = "value2"}));

                ExamineSession.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    var fields = s.Searcher.Doc(0).GetFields().Cast<Fieldable>().ToArray();
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name() == "item1"));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name() == "item2"));
                    Assert.AreEqual("value1", fields.Single(x => x.Name() == "item1").StringValue());
                    Assert.AreEqual("value2", fields.Single(x => x.Name() == "item2").StringValue());
                }
            }
        }

        [Test]
        public void Can_Have_Multiple_Values_In_Fields()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_29)))
            {

                indexer.IndexItems(new ValueSet(1, "content",
                    new Dictionary<string, List<object>>
                    {
                        {
                            "item1", new List<object> {"subval1", "subval2"}
                        },
                        {
                            "item2", new List<object> {"subval1", "subval2", "subval3"}
                        }
                    }));

                ExamineSession.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    var fields = s.Searcher.Doc(0).GetFields().Cast<Fieldable>().ToArray();
                    Assert.AreEqual(2, fields.Count(x => x.Name() == "item1"));
                    Assert.AreEqual(3, fields.Count(x => x.Name() == "item2"));

                    Assert.AreEqual("subval1", fields.Where(x => x.Name() == "item1").ElementAt(0).StringValue());
                    Assert.AreEqual("subval2", fields.Where(x => x.Name() == "item1").ElementAt(1).StringValue());

                    Assert.AreEqual("subval1", fields.Where(x => x.Name() == "item2").ElementAt(0).StringValue());
                    Assert.AreEqual("subval2", fields.Where(x => x.Name() == "item2").ElementAt(1).StringValue());
                    Assert.AreEqual("subval3", fields.Where(x => x.Name() == "item2").ElementAt(2).StringValue());
                }
            }
        }

        [Test]
        public void Can_Update_Document()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_29)))
            {

                indexer.IndexItems(new ValueSet(1, "content",
                    new {item1 = "value1", item2 = "value2"}));

                ExamineSession.WaitForChanges();

                indexer.IndexItems(new ValueSet(1, "content",
                    new {item1 = "value3", item2 = "value4"}));

                ExamineSession.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    var fields = s.Searcher.Doc(s.Searcher.MaxDoc() - 1).GetFields().Cast<Fieldable>().ToArray();
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name() == "item1"));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name() == "item2"));
                    Assert.AreEqual("value3", fields.Single(x => x.Name() == "item1").StringValue());
                    Assert.AreEqual("value4", fields.Single(x => x.Name() == "item2").StringValue());
                }
            }
        }

        [Test]
        public void Filters_Fields_With_Legacy_IndexCriteria()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(
                new IndexCriteria(new[]
                {
                    new StaticField("item1", false),
                    new StaticField("item3", false)
                }, Enumerable.Empty<IIndexField>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), null),
                luceneDir,
                new StandardAnalyzer(Version.LUCENE_29)))
            {
                indexer.IndexItems(new ValueSet(1, "content",
                new Dictionary<string, List<object>>
                {
                    {"item1", new List<object>(new[] {"value1"})},
                    {"item2", new List<object>(new[] {"value2"})}
                }));

                ExamineSession.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    var fields = s.Searcher.Doc(s.Searcher.MaxDoc() - 1).GetFields().Cast<Fieldable>().ToArray();
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name() == "item1"));
                    Assert.IsNull(fields.SingleOrDefault(x => x.Name() == "item2"));
                }
            }
            
        }

        [Test]
        public void Number_Field()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(
                new[]
                {
                    new FieldDefinition("item2", "number")
                },
                luceneDir,
                new StandardAnalyzer(Version.LUCENE_29)))
            {
                indexer.IndexItems(new ValueSet(1, "content",
                new Dictionary<string, List<object>>
                {
                    {"item1", new List<object>(new[] {"value1"})},
                    {"item2", new List<object>(new object[] {123456})}
                }));

                ExamineSession.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    var fields = s.Searcher.Doc(s.Searcher.MaxDoc() - 1).GetFields().Cast<Fieldable>().ToArray();

                    var valType = sc.GetValueType("item2");
                    Assert.AreEqual(typeof(Int32Type), valType.GetType());
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name() == "item2"));
                    //for a number type there will always be a sort field
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name() == LuceneIndexer.SortedFieldNamePrefix + "item2"));
                }
            }
            
        }
    }
}