using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{

    /// <summary>
    /// Indexes a raw string value - not analyzed
    /// </summary>
    public class RawStringType : IndexFieldValueTypeBase
    {
        private readonly Analyzer _analyzer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="store"></param>
        public RawStringType(string fieldName, ILoggerFactory logger, bool store = true)
            : base(fieldName, logger, store)
            => _analyzer = new KeywordAnalyzer();

        public override Analyzer Analyzer => _analyzer;

        protected override void AddSingleValue(Document doc, object value)
        {
            switch (value)
            {
                case IIndexableField f:
                    // https://lucene.apache.org/core/4_3_0/core/org/apache/lucene/index/IndexableField.html
                    // BinaryDocValuesField, ByteDocValuesField, DerefBytesDocValuesField, DoubleDocValuesField, DoubleField,
                    // Field, FloatDocValuesField, FloatField, IntDocValuesField, IntField, LongDocValuesField, LongField,
                    // NumericDocValuesField, PackedLongDocValuesField, ShortDocValuesField, SortedBytesDocValuesField,
                    // SortedDocValuesField, SortedSetDocValuesField, StoredField, StraightBytesDocValuesField, StringField, TextField
                    // https://solr.apache.org/guide/6_6/docvalues.html
                    doc.Add(f);
                    break;
                case TokenStream ts:
                    doc.Add(new TextField(FieldName, ts));
                    break;
                default:
                    if (TryConvert<string>(value, out var str))
                    {
                        doc.Add(new StringField(
                            FieldName,
                            str,
                            Store ? Field.Store.YES : Field.Store.NO));
                    }   
                    break;
            }
        }
    }

}
