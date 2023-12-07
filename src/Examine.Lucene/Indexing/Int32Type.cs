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
    /// Represents a Int32 <see cref="IndexFieldRangeValueType{T}" />
    /// </summary>
    public class Int32Type : IndexFieldRangeValueType<int>, IIndexFacetValueType
    {
        private readonly bool _isFacetable;
#pragma warning disable IDE0032 // Use auto property
        private readonly bool _taxonomyIndex;
#pragma warning restore IDE0032 // Use auto property

        /// <inheritdoc/>
        public Int32Type(string fieldName,bool isFacetable, bool taxonomyIndex, ILoggerFactory logger, bool store)
            : base(fieldName, logger, store)
        {
            _isFacetable = isFacetable;
            _taxonomyIndex = taxonomyIndex;
        }

        /// <inheritdoc/>
        [Obsolete("To be removed in Examine V5")]
#pragma warning disable RS0027 // API with optional parameter(s) should have the most parameters amongst its public overloads
        public Int32Type(string fieldName, ILoggerFactory logger, bool store = true)
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
                if (!TryConvert(objArr[0], out int parsedVal))
                {
                    return;
                }

                if (!TryConvert(objArr[1], out string[]? parsedPathVal))
                {
                    return;
                }

                doc.Add(new Int32Field(FieldName, parsedVal, Store ? Field.Store.YES : Field.Store.NO));

                doc.Add(new FacetField(FieldName, parsedPathVal));
                doc.Add(new NumericDocValuesField(FieldName, parsedVal));
                return;
            }
            base.AddValue(doc, value);
        }

        /// <inheritdoc/>
        protected override void AddSingleValue(Document doc, object value)
        {
            if (!TryConvert(value, out int parsedVal))
            {
                return;
            }

            doc.Add(new Int32Field(FieldName, parsedVal, Store ? Field.Store.YES : Field.Store.NO));

            if (_isFacetable && _taxonomyIndex)
            {
                doc.Add(new FacetField(FieldName, parsedVal.ToString()));
                doc.Add(new NumericDocValuesField(FieldName, parsedVal));
            }
            else if (_isFacetable && !_taxonomyIndex)
            {
                doc.Add(new SortedSetDocValuesFacetField(FieldName, parsedVal.ToString()));
                doc.Add(new NumericDocValuesField(FieldName, parsedVal));
            }
        }

        /// <inheritdoc/>
        public override Query? GetQuery(string query) => !TryConvert(query, out int parsedVal) ? null : GetQuery(parsedVal, parsedVal);

        /// <inheritdoc/>
        public override Query GetQuery(int? lower, int? upper, bool lowerInclusive = true, bool upperInclusive = true)
        {
            return NumericRangeQuery.NewInt32Range(FieldName,
                lower,
                upper, lowerInclusive, upperInclusive);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(IFacetExtractionContext facetExtractionContext, IFacetField field)
            => field.ExtractFacets(facetExtractionContext);
    }
}
