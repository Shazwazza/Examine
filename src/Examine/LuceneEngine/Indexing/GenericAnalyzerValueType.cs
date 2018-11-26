using System;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;

namespace Examine.LuceneEngine.Indexing
{
    /// <summary>
    /// A generic value type that will index a value based on the analyzer provider and will store the value with normal term vectors
    /// </summary>
    public class GenericAnalyzerValueType : IndexValueTypeBase
    {
        private readonly Analyzer _analyzer;

        public GenericAnalyzerValueType(string fieldName, Analyzer analyzer) : base(fieldName, true)
        {
            _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
        }

        public override void SetupAnalyzers(PerFieldAnalyzerWrapper analyzer)
        {
            base.SetupAnalyzers(analyzer);
            analyzer.AddAnalyzer(FieldName, _analyzer);
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            if (TryConvert<string>(value, out var str))
            {
                doc.Add(new Field(FieldName, str, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
            }   
        }
    }
}