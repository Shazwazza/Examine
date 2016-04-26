using System;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Indexing.ValueTypes;
using Examine.LuceneEngine.Providers;
using Examine.Session;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using NUnit.Framework;
using Version = Lucene.Net.Util.Version;

namespace Examine.Test.Index
{
    [TestFixture]
    public class LuceneIndexerTests
    {
        //TODO: Test all field definition types
        //TODO: Test FieldDefinition.IndexName and figure out what it does!

        [Test]
        public void Rebuild_Index()
        {
            using (var luceneDir = new RAMDirectory())            
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))
            {
                indexer.RebuildIndex();

                session.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    Assert.AreEqual(100, s.Searcher.IndexReader.NumDocs());
                }
            }
        }

        [Test]
        public void Reindex_Item_Type()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))
            {

                indexer.IndexAll("category0");

                session.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    Assert.AreEqual(50, s.Searcher.IndexReader.NumDocs());
                }

                indexer.IndexAll("category1");

                session.WaitForChanges();

                sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    Assert.AreEqual(100, s.Searcher.IndexReader.NumDocs());
                }
            }
        }

        [Test]
        public void Index_Exists()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))
            {
                Assert.IsTrue(indexer.IndexExists());
            }
        }

        [Test]
        public void Ensures_Doc_Is_Removed_If_Validation_Fails()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_30), 
                //custom validator
                validator: new ValueSetValidatorDelegate(set => !set.Values.ContainsKey("invalid"))))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))
            {

                var sc = indexer.SearcherContext;

                indexer.IndexItem(new ValueSet(1, "content",
                    new Dictionary<string, List<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new[] {"value2"})}
                    }));
                session.WaitForChanges();
                
                using (var s = sc.GetSearcher())
                {
                    Assert.AreEqual(1, s.Searcher.IndexReader.NumDocs());
                }

                //re-index with a value that will be invalid for the indexer - thus it should be removed from the index
                indexer.IndexItem(new ValueSet(1, "content",
                    new Dictionary<string, List<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new[] {"value2"})},
                        {"invalid", new List<object>(new[] {"this is invalid!"})}
                    }));
                session.WaitForChanges();

                using (var s = sc.GetSearcher())
                {
                    Assert.AreEqual(0, s.Searcher.IndexReader.NumDocs());
                }
            }
        }

        [Test]
        public void Can_Add_One_Document()
        {
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))
            {

                indexer.IndexItem(new ValueSet(1, "content",
                    new Dictionary<string, List<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new[] {"value2"})}
                    }));

                session.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    Assert.AreEqual(1, s.Searcher.IndexReader.NumDocs());
                }
            }
        }

        [Test]
        public void Can_Cancel_Indexing_Document()
        {
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))
            {
                EventHandler<TransformingIndexDataEventArgs> indexerOnTransformingIndexValues = (sender, args) =>
                {
                    if (args.IndexItem.ValueSet.Values.ContainsKey("blah"))
                    {
                        args.Cancel = true;
                    }
                };

                try
                {                    
                    indexer.TransformingIndexValues += indexerOnTransformingIndexValues;

                    indexer.IndexItem(new ValueSet(1, "content",
                        new Dictionary<string, List<object>>
                        {
                            {"blah", new List<object>(new[] {"value1"})},
                            {"item2", new List<object>(new[] {"value2"})}
                        }));

                    indexer.IndexItem(new ValueSet(2, "content",
                        new Dictionary<string, List<object>>
                        {
                            {"item2", new List<object>(new[] {"value2"})}
                        }));

                    session.WaitForChanges();

                    var sc = indexer.SearcherContext;
                    using (var s = sc.GetSearcher())
                    {
                        Assert.AreEqual(1, s.Searcher.IndexReader.NumDocs());
                    }
                }
                finally
                {
                    indexer.TransformingIndexValues -= indexerOnTransformingIndexValues;
                }
            }
        }

        [Test]
        public void Can_Cancel_Indexing_Document_Legacy_Event()
        {
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))
            {
                EventHandler<IndexingNodeEventArgs> indexerOnNodeIndexing = (sender, args) =>
                {
                    if (args.Fields.ContainsKey("blah"))
                    {
                        args.Cancel = true;
                    }
                };

                try
                {   
                    indexer.NodeIndexing += indexerOnNodeIndexing;

                    indexer.IndexItem(new ValueSet(1, "content",
                        new Dictionary<string, List<object>>
                        {
                            {"blah", new List<object>(new[] {"value1"})},
                            {"item2", new List<object>(new[] {"value2"})}
                        }));

                    indexer.IndexItem(new ValueSet(2, "content",
                        new Dictionary<string, List<object>>
                        {
                            {"item2", new List<object>(new[] {"value2"})}
                        }));

                    session.WaitForChanges();

                    var sc = indexer.SearcherContext;
                    using (var s = sc.GetSearcher())
                    {
                        Assert.AreEqual(1, s.Searcher.IndexReader.NumDocs());
                    }
                }
                finally
                {
                    indexer.NodeIndexing -= indexerOnNodeIndexing;
                }
            }
        }

        [Test]
        public void Can_Add_Same_Document_Twice_Without_Duplication()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))

            {

                var value = new ValueSet(1, "content",
                    new Dictionary<string, List<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new[] {"value2"})}
                    });

                indexer.IndexItem(value);
                session.WaitForChanges();

                indexer.IndexItem(value);
                session.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    Assert.AreEqual(1, s.Searcher.IndexReader.NumDocs());
                }
            }
        }

        [Test]
        public void Can_Add_Multiple_Docs()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))

            {

                for (var i = 0; i < 10; i++)
                {
                    indexer.IndexItem(new ValueSet(i, "content",
                        new Dictionary<string, List<object>>
                        {
                            {"item1", new List<object>(new[] {"value1"})},
                            {"item2", new List<object>(new[] {"value2"})}
                        }));
                }

                session.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    Assert.AreEqual(10, s.Searcher.IndexReader.NumDocs());
                }
            }
        }

        [Test]
        public void Can_Delete()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))

            {

                for (var i = 0; i < 10; i++)
                {
                    indexer.IndexItem(new ValueSet(i, "content",
                        new Dictionary<string, List<object>>
                        {
                            {"item1", new List<object>(new[] {"value1"})},
                            {"item2", new List<object>(new[] {"value2"})}
                        }));
                }
                indexer.DeleteFromIndex("9");

                session.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    Assert.AreEqual(9, s.Searcher.IndexReader.NumDocs());
                }
            }
        }

        [Test]
        public void Can_Add_Doc_With_Fields()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))

            {

                indexer.IndexItem(new ValueSet(1, "content",
                    new Dictionary<string, List<object>>
                    {
                        {"item1", new List<object>(new[] {"value1"})},
                        {"item2", new List<object>(new[] {"value2"})}
                    }));

                session.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    var fields = s.Searcher.Doc(0).GetFields().ToArray();
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item1"));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item2"));
                    Assert.AreEqual("value1", fields.Single(x => x.Name == "item1").StringValue);
                    Assert.AreEqual("value2", fields.Single(x => x.Name == "item2").StringValue);
                }
            }
        }

        [Test]
        public void Can_Add_Doc_With_Easy_Fields()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))

            {

                indexer.IndexItem(new ValueSet(1, "content",
                    new {item1 = "value1", item2 = "value2"}));

                session.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    var fields = s.Searcher.Doc(0).GetFields().ToArray();
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item1"));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item2"));
                    Assert.AreEqual("value1", fields.Single(x => x.Name == "item1").StringValue);
                    Assert.AreEqual("value2", fields.Single(x => x.Name == "item2").StringValue);
                }
            }
        }

        [Test]
        public void Can_Have_Multiple_Values_In_Fields()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))

            {

                indexer.IndexItem(new ValueSet(1, "content",
                    new Dictionary<string, List<object>>
                    {
                        {
                            "item1", new List<object> {"subval1", "subval2"}
                        },
                        {
                            "item2", new List<object> {"subval1", "subval2", "subval3"}
                        }
                    }));

                session.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    var fields = s.Searcher.Doc(0).GetFields().ToArray();
                    Assert.AreEqual(2, fields.Count(x => x.Name == "item1"));
                    Assert.AreEqual(3, fields.Count(x => x.Name == "item2"));

                    Assert.AreEqual("subval1", fields.Where(x => x.Name == "item1").ElementAt(0).StringValue);
                    Assert.AreEqual("subval2", fields.Where(x => x.Name == "item1").ElementAt(1).StringValue);

                    Assert.AreEqual("subval1", fields.Where(x => x.Name == "item2").ElementAt(0).StringValue);
                    Assert.AreEqual("subval2", fields.Where(x => x.Name == "item2").ElementAt(1).StringValue);
                    Assert.AreEqual("subval3", fields.Where(x => x.Name == "item2").ElementAt(2).StringValue);
                }
            }
        }

        [Test]
        public void Can_Update_Document()
        {
            using (var luceneDir = new RAMDirectory())      
            using (var indexer = new TestIndexer(luceneDir, new StandardAnalyzer(Version.LUCENE_30)))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))

            {

                indexer.IndexItem(new ValueSet(1, "content",
                    new {item1 = "value1", item2 = "value2"}));

                session.WaitForChanges();

                indexer.IndexItem(new ValueSet(1, "content",
                    new {item1 = "value3", item2 = "value4"}));

                session.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    var fields = s.Searcher.Doc(s.Searcher.MaxDoc - 1).GetFields().ToArray();
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item1"));
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item2"));
                    Assert.AreEqual("value3", fields.Single(x => x.Name == "item1").StringValue);
                    Assert.AreEqual("value4", fields.Single(x => x.Name == "item2").StringValue);
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
                new StandardAnalyzer(Version.LUCENE_30)))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))

            {
                indexer.IndexItem(new ValueSet(1, "content",
                new Dictionary<string, List<object>>
                {
                    {"item1", new List<object>(new[] {"value1"})},
                    {"item2", new List<object>(new[] {"value2"})}
                }));

                session.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    var fields = s.Searcher.Doc(s.Searcher.MaxDoc - 1).GetFields().ToArray();
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item1"));
                    Assert.IsNull(fields.SingleOrDefault(x => x.Name == "item2"));
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
                new StandardAnalyzer(Version.LUCENE_30)))
            using (var session = new ThreadScopedIndexSession(indexer.SearcherContext))

            {
                indexer.IndexItem(new ValueSet(1, "content",
                new Dictionary<string, List<object>>
                {
                    {"item1", new List<object>(new[] {"value1"})},
                    {"item2", new List<object>(new object[] {123456})}
                }));

                session.WaitForChanges();

                var sc = indexer.SearcherContext;
                using (var s = sc.GetSearcher())
                {
                    var fields = s.Searcher.Doc(s.Searcher.MaxDoc - 1).GetFields().ToArray();

                    var valType = sc.GetValueType("item2");
                    Assert.AreEqual(typeof(Int32Type), valType.GetType());
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == "item2"));
                    //for a number type there will always be a sort field
                    Assert.IsNotNull(fields.SingleOrDefault(x => x.Name == LuceneIndexer.SortedFieldNamePrefix + "item2"));
                }
            }
            
        }
    }

    internal class StaticField : IIndexField
    {
        public StaticField(string name, bool enableSorting, string type)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentNullException("type");

            Type = type;
            EnableSorting = enableSorting;
            Name = name;
        }

        public StaticField(string name, bool enableSorting)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

            Type = "fulltext";
            EnableSorting = enableSorting;
            Name = name;
        }

        [Obsolete("Use another constructor that does not specify an IndexType which is no longer used")]
        public StaticField(string name, FieldIndexTypes indexType, bool enableSorting, string type)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentNullException("type");

            Type = type;
            EnableSorting = enableSorting;
            IndexType = indexType;
            Name = name;
        }

        [Obsolete("Use another constructor that does not specify an IndexType which is no longer used")]
        public StaticField(string name, FieldIndexTypes indexType, bool enableSorting)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

            Type = "fulltext";
            EnableSorting = enableSorting;
            IndexType = indexType;
            Name = name;
        }

        public string Name { get; set; }

        [Obsolete("This is no longer used")]
        public FieldIndexTypes IndexType { get; private set; }

        public bool EnableSorting { get; set; }
        public string Type { get; set; }
    }
}