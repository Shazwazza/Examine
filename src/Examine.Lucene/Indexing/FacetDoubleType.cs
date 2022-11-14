using Lucene.Net.Documents;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    public class FacetDoubleType : DoubleType
    {
        public FacetDoubleType(string fieldName, ILoggerFactory logger, bool store = true) : base(fieldName, logger, store)
        {
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            base.AddSingleValue(doc, value);

            if (!TryConvert(value, out double parsedVal))
                return;

            doc.Add(new DoubleDocValuesField(FieldName, parsedVal));
        }
    }
}
