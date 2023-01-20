using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Lucene.Indexing;
using Examine.Suggest;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search.Spell;
using Lucene.Net.Search.Suggest;
using Lucene.Net.Search.Suggest.Analyzing;
using Lucene.Net.Util;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Lucene Index based Suggester
    /// </summary>
    public class LuceneSuggester : BaseLuceneSuggester, IDisposable
    {
        private readonly ReaderManager _readerManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private readonly SuggesterDefinitionCollection _suggesterDefinitions;
        private bool _disposedValue;

        private Dictionary<string, Lookup> _suggesters = new Dictionary<string, Lookup>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the Suggester</param>
        /// <param name="readerManager">Retrieves a IndexReaderReference for the index the Suggester is for</param>
        /// <param name="fieldValueTypeCollection">Index Field Types</param>
        /// <param name="suggestionSearchAnalyzer">Search time Analyzer</param>
        public LuceneSuggester(string name, ReaderManager readerManager, FieldValueTypeCollection fieldValueTypeCollection, SuggesterDefinitionCollection suggesterDefinitions)
            : base(name)
        {
            _readerManager = readerManager;
            _fieldValueTypeCollection = fieldValueTypeCollection;
            _suggesterDefinitions = suggesterDefinitions;
            BuildSuggesters();
        }

        public void RebuildSuggesters() => BuildSuggesters(true);

        protected virtual void BuildSuggesters(bool rebuild = false)
        {
            foreach (var suggesterDefintion in _suggesterDefinitions)
            {
                if (!rebuild && _suggesters.ContainsKey(suggesterDefintion.Name))
                {
                    throw new InvalidOperationException("Can not register more than one Suggester with the same name");
                }
                switch (suggesterDefintion.SuggesterMode)
                {
                    case ExamineLuceneSuggesterNames.AnalyzingInfixSuggester:
                        var lookupAnalyzingInfix = BuildAnalyzingInfixSuggesterLookup(suggesterDefintion,rebuild, true);
                        if (!rebuild)
                        {
                            _suggesters.Add(suggesterDefintion.Name, lookupAnalyzingInfix);
                        }
                        break;
                    case ExamineLuceneSuggesterNames.AnalyzingSuggester:
                        var lookupAnalyzing = BuildAnalyzingSuggesterLookup(suggesterDefintion, rebuild);
                        if (!rebuild)
                        {
                            _suggesters.Add(suggesterDefintion.Name, lookupAnalyzing);
                        }
                        break;
                    case ExamineLuceneSuggesterNames.FuzzySuggester:
                        var lookupFuzzy = BuildFuzzySuggesterLookup(suggesterDefintion, rebuild);
                        if (!rebuild)
                        {
                            _suggesters.Add(suggesterDefintion.Name, lookupFuzzy);
                        }
                        break;
                    case ExamineLuceneSuggesterNames.DirectSpellChecker:
                        break;
                    case ExamineLuceneSuggesterNames.DirectSpellChecker_JaroWinklerDistance:
                        break;
                    case ExamineLuceneSuggesterNames.DirectSpellChecker_LevensteinDistance:
                        break;
                    case ExamineLuceneSuggesterNames.DirectSpellChecker_NGramDistance:
                        break;
                    default:
                        throw new InvalidOperationException("Unknown SuggesterMode");
                }
            }
        }

        public override ISuggesterContext GetSuggesterContext() => new SuggesterContext(_readerManager, _fieldValueTypeCollection, _suggesterDefinitions, _suggesters);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var suggester in _suggesters.OfType<IDisposable>())
                    {
                        suggester?.Dispose();
                    }
                    _readerManager.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected Lookup BuildAnalyzingInfixSuggesterLookup(SuggesterDefinition suggesterDefinition, bool rebuild, bool highlight = true)
        {
            string field = suggesterDefinition.SourceFields.First();
            var fieldValue = GetFieldValueType(field);
            var indexTimeAnalyzer = fieldValue.Analyzer;


            AnalyzingInfixSuggester suggester = null;
            Analyzer queryTimeAnalyzer = null;
            LuceneSuggesterDefinition luceneSuggesterDefinition = suggesterDefinition as LuceneSuggesterDefinition;

            LuceneDirectory luceneDictionary = luceneSuggesterDefinition.SuggesterDirectoryFactory.CreateDirectory(suggesterDefinition.Name.Replace(".", "_"), false);
            var luceneVersion = LuceneVersion.LUCENE_48;

            if (rebuild)
            {
                suggester = _suggesters[suggesterDefinition.Name] as AnalyzingInfixSuggester;
            }
            else if (queryTimeAnalyzer != null)
            {
                suggester = new AnalyzingInfixSuggester(luceneVersion, luceneDictionary, indexTimeAnalyzer, queryTimeAnalyzer, AnalyzingInfixSuggester.DEFAULT_MIN_PREFIX_CHARS);
            }
            else
            {
                suggester = new AnalyzingInfixSuggester(luceneVersion, luceneDictionary, indexTimeAnalyzer);
            }
            using (var readerReference = new IndexReaderReference(_readerManager))
            {
                var lookupDictionary = new LuceneDictionary(readerReference.IndexReader, field);
                suggester.Build(lookupDictionary);
            }
            return suggester;
        }

        protected Lookup BuildFuzzySuggesterLookup(SuggesterDefinition suggesterDefinition,bool rebuild)
        {
            string field = suggesterDefinition.SourceFields.First();
            var fieldValue = GetFieldValueType(field);
            var indexTimeAnalyzer = fieldValue.Analyzer;

            FuzzySuggester suggester;
            Analyzer queryTimeAnalyzer = null;
            if (rebuild)
            {
                suggester = _suggesters[suggesterDefinition.Name] as FuzzySuggester;
            }
            else if (queryTimeAnalyzer != null)
            {
                suggester = new FuzzySuggester(indexTimeAnalyzer, queryTimeAnalyzer);
            }
            else
            {
                suggester = new FuzzySuggester(indexTimeAnalyzer);
            }
            using (var readerReference = new IndexReaderReference(_readerManager))
            {
                LuceneDictionary luceneDictionary = new LuceneDictionary(readerReference.IndexReader, field);
                suggester.Build(luceneDictionary);
            }

            return suggester;
        }

        protected Lookup BuildAnalyzingSuggesterLookup(SuggesterDefinition suggesterDefinition, bool rebuild)
        {
            string field = suggesterDefinition.SourceFields.First();
            var fieldValue = GetFieldValueType(field);
            var indexTimeAnalyzer = fieldValue.Analyzer;
            AnalyzingSuggester suggester;
            Analyzer queryTimeAnalyzer = null;
            if (rebuild)
            {
                suggester = _suggesters[suggesterDefinition.Name] as AnalyzingSuggester;
            }
            else if (queryTimeAnalyzer != null)
            {
                suggester = new AnalyzingSuggester(indexTimeAnalyzer, queryTimeAnalyzer);
            }
            else
            {
                suggester = new AnalyzingSuggester(indexTimeAnalyzer);
            }

            using (var readerReference = new IndexReaderReference(_readerManager))
            {
                LuceneDictionary luceneDictionary = new LuceneDictionary(readerReference.IndexReader, field);
                suggester.Build(luceneDictionary);
            }

            return suggester;
        }

        private IIndexFieldValueType GetFieldValueType(string fieldName)
        {
            //Get the value type for the field, or use the default if not defined
            return _fieldValueTypeCollection.GetValueType(
                fieldName,
                _fieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.FullText));
        }
    }
}
