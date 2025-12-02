using System;
using System.Collections.Generic;
using System.IO;
using Examine.Lucene.Analyzers;
using Examine.Lucene.Providers;
using Examine.Lucene.Search;
using Examine.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Documents;
using Lucene.Net.Facet;
using Lucene.Net.Facet.SortedSet;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    /// <summary>
    /// Default implementation for full text searching
    /// </summary>
    /// <remarks>
    /// By default will use a <see cref="CultureInvariantStandardAnalyzer"/> to perform the search and it will
    /// do an exact match search if the term is less than 4 chars, else it will do a full text search on the phrase
    /// with a higher boost, then 
    /// </remarks>
    public class FullTextType : IndexFieldValueTypeBase, IIndexFacetValueType
    {
        private readonly bool _sortable;
        private readonly Analyzer _analyzer;
        private readonly bool _isFacetable;
#pragma warning disable IDE0032 // Use auto property
        private readonly bool _taxonomyIndex;
#pragma warning restore IDE0032 // Use auto property

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="logger"></param>
        /// <param name="sortable"></param>
        /// <param name="isFacetable"></param>
        /// <param name="analyzer">
        /// <param name="taxonomyIndex"></param>
        /// Defaults to <see cref="CultureInvariantStandardAnalyzer"/>
        /// </param>
        public FullTextType(string fieldName, bool isFacetable, bool taxonomyIndex, bool sortable, ILoggerFactory logger, Analyzer analyzer)
            : base(fieldName, logger, true)
        {
            _sortable = sortable;
            _analyzer = analyzer;
            _isFacetable = isFacetable;
            _taxonomyIndex = taxonomyIndex;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="logger"></param>
        /// <param name="analyzer">
        /// Defaults to <see cref="CultureInvariantStandardAnalyzer"/>
        /// </param>
        /// <param name="sortable"></param>
        // [Obsolete("To be removed in Examine V5")] // TODO: Why?
#pragma warning disable RS0027 // API with optional parameter(s) should have the most parameters amongst its public overloads
        public FullTextType(string fieldName, ILoggerFactory logger, Analyzer? analyzer = null, bool sortable = false)
#pragma warning restore RS0027 // API with optional parameter(s) should have the most parameters amongst its public overloads
            : this(fieldName, false, false, sortable, logger, analyzer ?? new CultureInvariantStandardAnalyzer())
        {
        }

        /// <summary>
        /// Can be sorted by a concatenated field name since to be sortable it cannot be analyzed
        /// </summary>
        public override string? SortableFieldName => _sortable ? ExamineFieldNames.SortedFieldNamePrefix + FieldName : null;

        /// <inheritdoc/>
        public override Analyzer Analyzer => _analyzer;

        /// <inheritdoc/>
        public bool IsTaxonomyFaceted => _taxonomyIndex;

        /// <inheritdoc/>
        public override void AddValue(Document doc, object? value)
        {
            // Support setting taxonomy path
            if (_isFacetable && _taxonomyIndex && value is object[] objArr && objArr != null && objArr.Length == 2)
            {
                if (!TryConvert(objArr[0], out string? str))
                {
                    return;
                }

                if (!TryConvert(objArr[1], out string[]? parsedPathVal))
                {
                    return;
                }

                doc.Add(new TextField(FieldName, str, Field.Store.YES));

                if (_sortable)
                {
                    //to be sortable it cannot be analyzed so we have to make a different field
                    doc.Add(new StringField(
                        ExamineFieldNames.SortedFieldNamePrefix + FieldName,
                        str,
                        Field.Store.YES));
                }

                doc.Add(new FacetField(FieldName, parsedPathVal));
                return;
            }
            base.AddValue(doc, value);
        }

        /// <inheritdoc/>
        protected override void AddSingleValue(Document doc, object value)
        {
            if (TryConvert<string>(value, out var str))
            {
                doc.Add(new TextField(FieldName, str, Field.Store.YES));

                if (_sortable)
                {
                    //to be sortable it cannot be analyzed so we have to make a different field
                    // TODO: Investigate https://lucene.apache.org/core/4_3_0/core/org/apache/lucene/document/SortedDocValuesField.html
                    doc.Add(new StringField(
                        ExamineFieldNames.SortedFieldNamePrefix + FieldName,
                        str,
                        Field.Store.YES));
                }

                if (_isFacetable && _taxonomyIndex)
                {
                    doc.Add(new FacetField(FieldName, str));
                }
                else if (_isFacetable && !_taxonomyIndex)
                {
                    doc.Add(new SortedSetDocValuesFacetField(FieldName, str));
                }
            }
        }

        /// <summary>
        /// Generates a full text query
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="query"></param>
        /// <param name="analyzer"></param>
        /// <returns></returns>
        public static Query? GenerateQuery(string fieldName, string query, Analyzer analyzer)
        {
            if (query == null)
            {
                return null;
            }

            var resultQuery = new BooleanQuery();
            var phraseQuery = new PhraseQuery { Slop = 0 };

            //var phraseQueryTerms = new List<Term>();

            //not much to search, only do exact match
            if (query.Length < 4)
            {
                phraseQuery.Add(new Term(fieldName, query));

                resultQuery.Add(phraseQuery, Occur.MUST);
                return resultQuery;
            }

            //add phrase match with boost, we will add the terms to the phrase below
            phraseQuery.Boost = 20;
            resultQuery.Add(phraseQuery, Occur.SHOULD);

            var tokenStream = analyzer.GetTokenStream("SearchText", new StringReader(query));
            var termAttribute = tokenStream.AddAttribute<ICharTermAttribute>();
            tokenStream.Reset();
            while (tokenStream.IncrementToken())
            {
                var term = termAttribute.ToString();

                //phraseQueryTerms.Add(new Term(fieldName, term));
                //phraseQuery.Add(new[] { new Term(fieldName, term) });
                phraseQuery.Add(new Term(fieldName, term));

                var exactMatch = new TermQuery(new Term(fieldName, term));

                //if the term is larger than 3, we'll do both exact match and wildcard/prefix
                if (term.Length >= 3)
                {
                    var innerQuery = new BooleanQuery();

                    //add exact match with boost
                    exactMatch.Boost = 10;
                    innerQuery.Add(exactMatch, Occur.SHOULD);

                    //add wildcard
                    var pq = new PrefixQuery(new Term(fieldName, term))
                    {
                        //needed so that wildcard searches will return a score
                        MultiTermRewriteMethod = MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE //new ErrorCheckingScoringBooleanQueryRewrite();
                    };
                    innerQuery.Add(pq, Occur.SHOULD);

                    resultQuery.Add(innerQuery, Occur.MUST);
                }
                else
                {
                    resultQuery.Add(exactMatch, Occur.MUST);
                }
            }

            tokenStream.End();
            tokenStream.Dispose();

            return resultQuery.Clauses.Count > 0 ? resultQuery : null;
        }

        /// <summary>
        /// Builds a full text search query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public override Query? GetQuery(string query) => GenerateQuery(FieldName, query, _analyzer);

        /// <inheritdoc/>
        public virtual IEnumerable<KeyValuePair<string, IFacetResult>> ExtractFacets(IFacetExtractionContext facetExtractionContext, IFacetField field)
            => field.ExtractFacets(facetExtractionContext);
    }
}
