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
using static Lucene.Net.Queries.Function.ValueSources.MultiFunction;

namespace Examine.Lucene.Indexing
{
    /// <summary>
    /// Represents a float/single <see cref="IndexFieldRangeValueType{T}"/>
    /// </summary>
    public class FloatType : IndexFieldRangeValueType<float>, IIndexFacetValueType
    {
        private readonly bool _isFacetable;
#pragma warning disable IDE0032 // Use auto property
        private readonly bool _taxonomyIndex;
#pragma warning restore IDE0032 // Use auto property

        /// <inheritdoc/>
        public FloatType(string fieldName, bool isFacetable, bool taxonomyIndex, ILoggerFactory logger, bool store)
            : base(fieldName, logger, store)
        {
            _isFacetable = isFacetable;
            _taxonomyIndex = taxonomyIndex;
        }

        /// <inheritdoc/>
        [Obsolete("To be removed in Examine V5")]
#pragma warning disable RS0027 // API with optional parameter(s) should have the most parameters amongst its public overloads
        public FloatType(string fieldName, ILoggerFactory logger, bool store = true)
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
                if (!TryConvert(objArr[0], out float parsedVal))
                {
                    return;
                }

                if (!TryConvert(objArr[1], out string[]? parsedPathVal))
                {
                    return;
                }
                
                doc.Add(new SingleField(FieldName, parsedVal, Store ? Field.Store.YES : Field.Store.NO));

                doc.Add(new FacetField(FieldName, parsedPathVal));
                doc.Add(new SingleDocValuesField(FieldName, parsedVal));
                return;
            }
            base.AddValue(doc, value);
        }

        /// <inheritdoc/>
        protected override void AddSingleValue(Document doc, object value)
        {
            if (!TryConvert(value, out float parsedVal))
            {
                return;
            }

            doc.Add(new SingleField(FieldName, parsedVal, Store ? Field.Store.YES : Field.Store.NO));

            if (_isFacetable && _taxonomyIndex)
            {
                doc.Add(new FacetField(FieldName, parsedVal.ToString()));
                doc.Add(new SingleDocValuesField(FieldName, parsedVal));
            }
            else if (_isFacetable && !_taxonomyIndex)
            {
                doc.Add(new SortedSetDocValuesFacetField(FieldName, parsedVal.ToString()));
                doc.Add(new SingleDocValuesField(FieldName, parsedVal));
            }
        }

        /// <inheritdoc/>
        public override Query? GetQuery(string query) => !TryConvert(query, out float parsedVal) ? null : GetQuery(parsedVal, parsedVal);

        /// <inheritdoc/>
        public override Query GetQuery(float? lower, float? upper, bool lowerInclusive = true, bool upperInclusive = true)
        {
            return NumericRangeQuery.NewSingleRange(FieldName,
                lower ?? float.MinValue,
                upper ?? float.MaxValue, lowerInclusive, upperInclusive);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(IFacetExtractionContext facetExtractionContext, IFacetField field)
            => field.ExtractFacets(facetExtractionContext);
    }

}
