using System.Collections.Generic;
using Examine.Lucene.Providers;
using Examine.Lucene.Search;
using Examine.Search;
using Lucene.Net.Documents;
using Lucene.Net.Facet;
using Lucene.Net.Facet.SortedSet;
using Lucene.Net.Search;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    public class SingleType : IndexFieldRangeValueType<float>, IIndexFacetValueType
    {
        private readonly bool _isFacetable;

        public SingleType(string fieldName, ILoggerFactory logger, bool store, bool isFacetable)
            : base(fieldName, logger, store)
        {
            _isFacetable = isFacetable;
        }

        public SingleType(string fieldName, ILoggerFactory logger, bool store = true)
            : base(fieldName, logger, store)
        {
            _isFacetable = false;
        }

        /// <summary>
        /// Can be sorted by the normal field name
        /// </summary>
        public override string SortableFieldName => FieldName;

        protected override void AddSingleValue(Document doc, object value)
        {
            if (!TryConvert(value, out float parsedVal))
                return;

            doc.Add(new DoubleField(FieldName,parsedVal, Store ? Field.Store.YES : Field.Store.NO));

            if (_isFacetable)
            {
                doc.Add(new SortedSetDocValuesFacetField(FieldName, parsedVal.ToString()));
                doc.Add(new SingleDocValuesField(FieldName, parsedVal));
            }
        }

        public override Query GetQuery(string query)
        {
            return !TryConvert(query, out float parsedVal) ? null : GetQuery(parsedVal, parsedVal);
        }

        public override Query GetQuery(float? lower, float? upper, bool lowerInclusive = true, bool upperInclusive = true)
        {
            return NumericRangeQuery.NewDoubleRange(FieldName,
                lower ?? float.MinValue,
                upper ?? float.MaxValue, lowerInclusive, upperInclusive);
        }

        public virtual void ExtractFacets(FacetsCollector facetsCollector, SortedSetDocValuesReaderState sortedSetReaderState, Dictionary<string, IFacetResult> facets, IFacetField field) => field.ExtractFacets(facetsCollector, sortedSetReaderState, facets);
    }
}
