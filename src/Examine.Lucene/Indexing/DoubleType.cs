using Examine.Lucene.Providers;
using Lucene.Net.Documents;
using Lucene.Net.Facet;
using Lucene.Net.Facet.SortedSet;
using Lucene.Net.Search;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    public class DoubleType : IndexFieldRangeValueType<double>
    {
        private readonly bool _isFacetable;
        private readonly bool _taxonomyIndex;

        public DoubleType(string fieldName, ILoggerFactory logger, bool store = true, bool isFacetable = false, bool taxonomyIndex = false)
            : base(fieldName, logger, store)
        {
            _isFacetable = isFacetable;
            _taxonomyIndex = taxonomyIndex;
        }

        /// <summary>
        /// Can be sorted by the normal field name
        /// </summary>
        public override string SortableFieldName => FieldName;

        public override void AddValue(Document doc, object value)
        {
            // Support setting taxonomy path
            if (_isFacetable && _taxonomyIndex && value is object[] objArr && objArr != null && objArr.Length == 2)
            {
                if (!TryConvert(objArr[0], out double parsedVal))
                    return;
                if (!TryConvert(objArr[1], out string[] parsedPathVal))
                    return;

                doc.Add(new DoubleField(FieldName, parsedVal, Store ? Field.Store.YES : Field.Store.NO));

                doc.Add(new FacetField(FieldName, parsedPathVal));
                doc.Add(new DoubleDocValuesField(FieldName, parsedVal));
                return;
            }
            base.AddValue(doc, value);
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            if (!TryConvert(value, out double parsedVal))
                return;

            doc.Add(new DoubleField(FieldName,parsedVal, Store ? Field.Store.YES : Field.Store.NO));

            if (_isFacetable && _taxonomyIndex)
            {
                doc.Add(new FacetField(FieldName, parsedVal.ToString()));
                doc.Add(new DoubleDocValuesField(FieldName, parsedVal));
            }
            else if (_isFacetable && !_taxonomyIndex)
            {
                doc.Add(new SortedSetDocValuesFacetField(FieldName, parsedVal.ToString()));
                doc.Add(new DoubleDocValuesField(FieldName, parsedVal));
            }
        }

        public override Query GetQuery(string query)
        {
            return !TryConvert(query, out double parsedVal) ? null : GetQuery(parsedVal, parsedVal);
        }

        public override Query GetQuery(double? lower, double? upper, bool lowerInclusive = true, bool upperInclusive = true)
        {
            return NumericRangeQuery.NewDoubleRange(FieldName,
                lower ?? double.MinValue,
                upper ?? double.MaxValue, lowerInclusive, upperInclusive);
        }
    }
}
