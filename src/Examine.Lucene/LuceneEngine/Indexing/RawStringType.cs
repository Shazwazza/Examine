using Lucene.Net.Analysis;
using Lucene.Net.Documents;

namespace Examine.LuceneEngine.Indexing
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
        public RawStringType(string fieldName, bool store = true)
            : base(fieldName, store)
        {
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            switch (value)
            {
                case IFieldable f:
                    doc.Add(f);
                    break;
                case TokenStream ts:
                    doc.Add(new Field(FieldName, ts));
                    break;
                default:
                    if (TryConvert<string>(value, out var str))
                    {
                        doc.Add(new Field(FieldName, str,
                            Store ? Field.Store.YES : Field.Store.NO,
                            Field.Index.NOT_ANALYZED,
                            Field.TermVector.NO));
                    }   
                    break;
            }
        }
    }

}
