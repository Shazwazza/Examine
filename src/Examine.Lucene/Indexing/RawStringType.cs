using Lucene.Net.Analysis;
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
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="store"></param>
        public RawStringType(string fieldName, ILoggerFactory logger, bool store = true)
            : base(fieldName, logger, store)
        {
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            switch (value)
            {
                case IIndexableField f:
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
