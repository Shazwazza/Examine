using System;
using Lucene.Net.Documents;
using Lucene.Net.Facet.SortedSet;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    public class FacetDateTimeType : DateTimeType
    {
        public FacetDateTimeType(string fieldName, ILoggerFactory logger, DateResolution resolution, bool store = true) : base(fieldName, logger, resolution, store)
        {
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            base.AddSingleValue(doc, value);

            if (!TryConvert(value, out DateTime parsedVal))
                return;

            var val = DateToLong(parsedVal);

            doc.Add(new SortedSetDocValuesFacetField(FieldName, val.ToString()));
            doc.Add(new NumericDocValuesField(FieldName, val));
        }
    }
}
