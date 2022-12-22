using System;
using Lucene.Net.Documents;
using Lucene.Net.Facet;
using Lucene.Net.Facet.SortedSet;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    public class FacetDateTimeType : DateTimeType
    {
        private readonly bool _taxonomyIndex;

        public FacetDateTimeType(string fieldName, ILoggerFactory logger, DateResolution resolution, bool store = true, bool taxonomyIndex = false) : base(fieldName, logger, resolution, store)
        {
            _taxonomyIndex = taxonomyIndex;
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            base.AddSingleValue(doc, value);

            if (!TryConvert(value, out DateTime parsedVal))
                return;

            var val = DateToLong(parsedVal);
            if (_taxonomyIndex)
            {
                doc.Add(new FacetField(FieldName, val.ToString()));
                doc.Add(new NumericDocValuesField(FieldName, val));
            }
            else
            {
                doc.Add(new SortedSetDocValuesFacetField(FieldName, val.ToString()));
                doc.Add(new NumericDocValuesField(FieldName, val));
            }
        }
    }
}
