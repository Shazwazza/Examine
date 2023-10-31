using System;
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
    /// <summary>
    /// Represents a Double <see cref="IndexFieldRangeValueType{T}"/>
    /// </summary>
    public class DoubleType : IndexFieldRangeValueType<double>, IIndexFacetValueType
    {
        private readonly bool _isFacetable;
#pragma warning disable IDE0032 // Use auto property
        private readonly bool _taxonomyIndex;
#pragma warning restore IDE0032 // Use auto property

        /// <inheritdoc/>
        public DoubleType(string fieldName, bool isFacetable, bool taxonomyIndex, ILoggerFactory logger, bool store)
            : base(fieldName, logger, store)
        {
            _isFacetable = isFacetable;
            _taxonomyIndex = taxonomyIndex;
        }

        /// <inheritdoc/>
        [Obsolete("To be removed in Examine V5")]
#pragma warning disable RS0027 // API with optional parameter(s) should have the most parameters amongst its public overloads
        public DoubleType(string fieldName, ILoggerFactory logger, bool store = true)
#pragma warning restore RS0027 // API with optional parameter(s) should have the most parameters amongst its public overloads
            : base(fieldName, logger, store)
        {
            _isFacetable = false;
        }

        /// <summary>
        /// Can be sorted by the normal field name
        /// </summary>
        public override string SortableFieldName => FieldName;

        /// <inheritdoc/>
        public bool IsTaxonomyFaceted => _taxonomyIndex;

        /// <inheritdoc/>
        public override void AddValue(Document doc, object? value)
        {
            // Support setting taxonomy path
            if (_isFacetable && _taxonomyIndex && value is object[] objArr && objArr != null && objArr.Length == 2)
            {
                if (!TryConvert(objArr[0], out double parsedVal))
                {
                    return;
                }

                if (!TryConvert(objArr[1], out string[]? parsedPathVal))
                {
                    return;
                }

                doc.Add(new DoubleField(FieldName, parsedVal, Store ? Field.Store.YES : Field.Store.NO));

                doc.Add(new FacetField(FieldName, parsedPathVal));
                doc.Add(new DoubleDocValuesField(FieldName, parsedVal));
                return;
            }
            base.AddValue(doc, value);
        }

        /// <inheritdoc/>
        protected override void AddSingleValue(Document doc, object value)
        {
            if (!TryConvert(value, out double parsedVal))
            {
                return;
            }

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

        /// <inheritdoc/>
        public override Query? GetQuery(string query)
        {
            if (!TryConvert(query, out double parsedVal))
            {
                return null;
            }
            else
            {
                return (Query?)GetQuery(parsedVal, parsedVal);
            }
        }

        /// <inheritdoc/>
        public override Query GetQuery(double? lower, double? upper, bool lowerInclusive = true, bool upperInclusive = true)
        {
            return NumericRangeQuery.NewDoubleRange(FieldName,
                lower ?? double.MinValue,
                upper ?? double.MaxValue, lowerInclusive, upperInclusive);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(IFacetExtractionContext facetExtractionContext, IFacetField field)
            => field.ExtractFacets(facetExtractionContext);
    }
}
