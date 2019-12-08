﻿using System.Collections.Generic;
using System.IO;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing
{
    /// <summary>
    /// Default implementation for full text searching
    /// </summary>
    /// <remarks>
    /// By default will use a <see cref="CultureInvariantStandardAnalyzer"/> to perform the search and it will
    /// do an exact match search if the term is less than 4 chars, else it will do a full text search on the phrase
    /// with a higher boost, then 
    /// </remarks>
    public class FullTextType : IndexFieldValueTypeBase
    {
        private readonly bool _sortable;
        private readonly Analyzer _analyzer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="analyzer">
        /// Defaults to <see cref="CultureInvariantStandardAnalyzer"/>
        /// </param>
        /// <param name="sortable"></param>
        public FullTextType(string fieldName, Analyzer analyzer = null, bool sortable = false)
            : base(fieldName, true)
        {
            _sortable = sortable;
            _analyzer = analyzer ?? new CultureInvariantStandardAnalyzer();
        }

        /// <summary>
        /// Can be sorted by a concatenated field name since to be sortable it cannot be analyzed
        /// </summary>
        public override string SortableFieldName => _sortable ? LuceneIndex.SortedFieldNamePrefix + FieldName : null;

        public override void SetupAnalyzers(PerFieldAnalyzerWrapper analyzer)
        {
            base.SetupAnalyzers(analyzer);

            analyzer.AddAnalyzer(FieldName, _analyzer);
        }

        protected override void AddSingleValue(Document doc, object value)
        {
            if (TryConvert<string>(value, out var str))
            {
                doc.Add(new Field(FieldName, str, Field.Store.YES, Field.Index.ANALYZED,
                Field.TermVector.WITH_POSITIONS_OFFSETS /* This is required for the fast vector highligher but will double the field size */ ));

                if (_sortable)
                {
                    //to be sortable it cannot be analyzed so we have to make a different field
                    doc.Add(new Field(LuceneIndex.SortedFieldNamePrefix + FieldName, str,
                        Field.Store.YES,
                        Field.Index.NOT_ANALYZED, Field.TermVector.NO));
                }
            }
        }

        public static Query GenerateQuery(string fieldName, string query, Analyzer analyzer)
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

            var tokenStream = analyzer.TokenStream("SearchText", new StringReader(query));
            var termAttribute = tokenStream.AddAttribute<ITermAttribute>();

            while (tokenStream.IncrementToken())
            {
                var term = termAttribute.Term;

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
                    var pq = new PrefixQuery(new Term(fieldName, term));
                    //needed so that wildcard searches will return a score
                    pq.RewriteMethod = MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE; //new ErrorCheckingScoringBooleanQueryRewrite();
                    innerQuery.Add(pq, Occur.SHOULD);

                    resultQuery.Add(innerQuery, Occur.MUST);
                }
                else
                {
                    resultQuery.Add(exactMatch, Occur.MUST);
                }
            }

            //phraseQuery.Add(phraseQueryTerms.ToArray());

            return resultQuery.Clauses.Count > 0 ? resultQuery : null;
        }

        /// <summary>
        /// Builds a full text search query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="searcher"></param>
        /// <returns></returns>
        public override Query GetQuery(string query, Searcher searcher)
        {
            return GenerateQuery(FieldName, query, _analyzer);
        }

    }
}
