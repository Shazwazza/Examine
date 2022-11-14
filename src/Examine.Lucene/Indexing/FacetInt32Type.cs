using Lucene.Net.Documents;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    public class FacetInt32Type : Int32Type
    {
        public FacetInt32Type(string fieldName, ILoggerFactory logger, bool store = true) : base(fieldName, logger, store)
        {
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            base.AddSingleValue(doc, value);

            if (!TryConvert(value, out int parsedVal))
                return;

            doc.Add(new NumericDocValuesField(FieldName, parsedVal));
        }
    }
}
