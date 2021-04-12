using System;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Examine.Test
{
    public class TestIndex : LuceneIndex
    {
        public TestIndex(FieldDefinitionCollection fieldDefinitions, Directory luceneDirectory, Analyzer analyzer, IValueSetValidator validator = null, IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypesFactory = null)
            : base("testIndexer", luceneDirectory, fieldDefinitions, analyzer, validator, indexValueTypesFactory)
        {
            RunAsync = false;
        }

        public TestIndex(Directory luceneDirectory, Analyzer defaultAnalyzer, IValueSetValidator validator = null)
            : base("testIndexer", luceneDirectory, new FieldDefinitionCollection(), defaultAnalyzer, validator)
        {
            RunAsync = false;
        }

        public TestIndex(IndexWriter writer, IValueSetValidator validator = null)
            : base("testIndexer", new FieldDefinitionCollection(), writer, validator)
        {
        }

        public IEnumerable<ValueSet> AllData()
        {
            var data = new List<ValueSet>();
            for (int i = 0; i < 100; i++)
            {
                data.Add(ValueSet.FromObject(i.ToString(), "category" + (i % 2), new { item1 = "value" + i, item2 = "value" + i }));
            }
            return data;
        }
    }
}