using System;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Documents;

namespace Examine.LuceneEngine.Indexing
{
    /// <summary>
    /// A generic value type that will index a value based on the analyzer provider and will store the value with normal term vectors
    /// </summary>
    public class GenericAnalyzerFieldValueType : IndexFieldValueTypeBase
    {
        private readonly Analyzer _analyzer;
        private readonly bool _sortable;

        public GenericAnalyzerFieldValueType(string fieldName, Analyzer analyzer, bool sortable = false) : base(fieldName, true)
        {
            _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
            _sortable = sortable;
        }

        /// <summary>
        /// Can be sorted by a concatenated field name since to be sortable it cannot be analyzed
        /// </summary>
        public override string SortableFieldName => _sortable ? ExamineFieldNames.SortedFieldNamePrefix + FieldName : null;

        public override void SetupAnalyzers(PerFieldAnalyzerWrapper analyzer)
        {
            base.SetupAnalyzers(analyzer);
         
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            if (TryConvert<string>(value, out var str))
            {
                doc.Add(new Field(FieldName, str, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));

                if (_sortable)
                {
                    //to be sortable it cannot be analyzed so we have to make a different field
                    doc.Add(new Field(ExamineFieldNames.SortedFieldNamePrefix + FieldName, str,
                        Field.Store.YES,
                        Field.Index.NOT_ANALYZED, Field.TermVector.NO));
                }
            }   
        }
    }
}