using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Examine.Lucene.Analyzers;
using Examine.Lucene.Providers;
using Examine.Lucene.Suggest;
using Examine.Suggest;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Spell;
using Lucene.Net.Search.Suggest;
using Lucene.Net.Search.Suggest.Analyzing;
using Microsoft.Extensions.Logging;
using static Lucene.Net.Search.Suggest.Lookup;

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
    public class FullTextType : IndexFieldValueTypeBase
    {
        private readonly bool _sortable;
        private readonly bool _suggestable;
        private readonly Func<IIndexReaderReference, SuggestionOptions, string, LuceneSuggestionResults> _lookup;
        private readonly Analyzer _analyzer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="analyzer">
        /// Defaults to <see cref="CultureInvariantStandardAnalyzer"/>
        /// </param>
        /// <param name="sortable"></param>
        public FullTextType(string fieldName, ILoggerFactory logger, Analyzer analyzer = null, bool sortable = false, bool suggestable = false, Func<IIndexReaderReference, SuggestionOptions, string, LuceneSuggestionResults> lookup = null)
            : base(fieldName, logger, true)
        {
            _sortable = sortable;
            _suggestable = suggestable;
            _analyzer = analyzer ?? new CultureInvariantStandardAnalyzer();
            _lookup = lookup;

        }

        /// <summary>
        /// Can be sorted by a concatenated field name since to be sortable it cannot be analyzed
        /// </summary>
        public override string SortableFieldName => _sortable ? ExamineFieldNames.SortedFieldNamePrefix + FieldName : null;

        public override Analyzer Analyzer => _analyzer;

        public override Func<IIndexReaderReference, SuggestionOptions, string, LuceneSuggestionResults> Lookup => _suggestable ? _lookup : null;

        protected override void AddSingleValue(Document doc, object value)
        {
            if (TryConvert<string>(value, out var str))
            {
                doc.Add(new TextField(FieldName, str, Field.Store.YES));

                if (_sortable)
                {
                    //to be sortable it cannot be analyzed so we have to make a different field
                    doc.Add(new StringField(
                        ExamineFieldNames.SortedFieldNamePrefix + FieldName,
                        str,
                        Field.Store.YES));
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
                    var pq = new PrefixQuery(new Term(fieldName, term));
                    //needed so that wildcard searches will return a score
                    pq.MultiTermRewriteMethod = MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE; //new ErrorCheckingScoringBooleanQueryRewrite();
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
        /// 
        /// <returns></returns>
        public override Query GetQuery(string query)
        {
            return GenerateQuery(FieldName, query, _analyzer);
        }

        /// <summary>
        /// Analyzing Suggester Lookup
        /// </summary>
        public static LuceneSuggestionResults ExecuteAnalyzingSuggester(IIndexReaderReference readerReference, SuggestionOptions suggestionOptions, string searchText, string fieldName, Analyzer indexTimeAnalyzer)
        {
            LuceneDictionary luceneDictionary = new LuceneDictionary(readerReference.IndexReader, fieldName);
            AnalyzingSuggester analyzingSuggester;
            var onlyMorePopular = false;
            if (suggestionOptions is LuceneSuggestionOptions luceneSuggestionOptions)
            {
                if (luceneSuggestionOptions.Analyzer != null)
                {
                    analyzingSuggester = new AnalyzingSuggester(indexTimeAnalyzer, luceneSuggestionOptions.Analyzer);
                }
                else
                {
                    analyzingSuggester = new AnalyzingSuggester(indexTimeAnalyzer);
                }
                if (luceneSuggestionOptions.SuggestionMode == LuceneSuggestionOptions.SuggestMode.SUGGEST_MORE_POPULAR)
                {
                    onlyMorePopular = true;
                }
            }
            else
            {
                analyzingSuggester = new AnalyzingSuggester(indexTimeAnalyzer);
            }

            analyzingSuggester.Build(luceneDictionary);
            var lookupResults = analyzingSuggester.DoLookup(searchText, onlyMorePopular, suggestionOptions.Top);
            var results = lookupResults.Select(x => new SuggestionResult(x.Key, x.Value));
            LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
            return suggestionResults;
        }

        /// <summary>
        /// Fuzzy Suggester Lookup
        /// </summary>
        public static LuceneSuggestionResults ExecuteFuzzySuggester(IIndexReaderReference readerReference, SuggestionOptions suggestionOptions, string searchText, string fieldName, Analyzer indexTimeAnalyzer)
        {
            LuceneDictionary luceneDictionary = new LuceneDictionary(readerReference.IndexReader, fieldName);
            FuzzySuggester analyzingSuggester;
            var onlyMorePopular = false;
            if (suggestionOptions is LuceneSuggestionOptions luceneSuggestionOptions)
            {
                if (luceneSuggestionOptions.Analyzer != null)
                {
                    analyzingSuggester = new FuzzySuggester(indexTimeAnalyzer, luceneSuggestionOptions.Analyzer);
                }
                else
                {
                    analyzingSuggester = new FuzzySuggester(indexTimeAnalyzer);
                }
                if (luceneSuggestionOptions.SuggestionMode == LuceneSuggestionOptions.SuggestMode.SUGGEST_MORE_POPULAR)
                {
                    onlyMorePopular = true;
                }
            }
            else
            {
                analyzingSuggester = new FuzzySuggester(indexTimeAnalyzer);
            }

            analyzingSuggester.Build(luceneDictionary);
            var lookupResults = analyzingSuggester.DoLookup(searchText, onlyMorePopular, suggestionOptions.Top);
            var results = lookupResults.Select(x => new SuggestionResult(x.Key, x.Value));
            LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
            return suggestionResults;
        }

        /// <summary>
        /// Fuzzy Suggester Lookup
        /// </summary>
        public static LuceneSuggestionResults ExecuteDirectSpellChecker(IIndexReaderReference readerReference, SuggestionOptions suggestionOptions, string searchText, string fieldName, Analyzer indexTimeAnalyzer)
        {
            DirectSpellChecker spellchecker = new DirectSpellChecker();
            if (suggestionOptions.SuggesterName.Equals(ExamineLuceneSuggesterNames.DirectSpellChecker_JaroWinklerDistance, StringComparison.InvariantCultureIgnoreCase))
            {
                spellchecker.Distance = new JaroWinklerDistance();
            }
            else if (suggestionOptions.SuggesterName.Equals(ExamineLuceneSuggesterNames.DirectSpellChecker_LevensteinDistance, StringComparison.InvariantCultureIgnoreCase))
            {
                spellchecker.Distance = new LevensteinDistance();
            }
            else if (suggestionOptions.SuggesterName.Equals(ExamineLuceneSuggesterNames.DirectSpellChecker_NGramDistance, StringComparison.InvariantCultureIgnoreCase))
            {
                spellchecker.Distance = new NGramDistance();
            }

            var suggestMode = SuggestMode.SUGGEST_WHEN_NOT_IN_INDEX;
            if (suggestionOptions is LuceneSuggestionOptions luceneSuggestionOptions)
            {
                suggestMode = (SuggestMode)luceneSuggestionOptions.SuggestionMode;
            }

            var lookupResults = spellchecker.SuggestSimilar(new Term(fieldName, searchText),
                                                            suggestionOptions.Top,
                                                            readerReference.IndexReader,
                                                            suggestMode);
            var results = lookupResults.Select(x => new SuggestionResult(x.String, x.Score, x.Freq));
            LuceneSuggestionResults suggestionResults = new LuceneSuggestionResults(results.ToArray());
            return suggestionResults;
        }
    }
}
