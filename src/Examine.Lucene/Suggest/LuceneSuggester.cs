using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Index;

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

        private Dictionary<string, ILookupExecutor> _suggesters = new Dictionary<string, ILookupExecutor>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the Suggester</param>
        /// <param name="readerManager">Retrieves a IndexReaderReference for the index the Suggester is for</param>
        /// <param name="fieldValueTypeCollection">Index Field Types</param>
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
                var luceneSuggesterDefinition = suggesterDefintion as LuceneSuggesterDefinition;
                if (luceneSuggesterDefinition != null)
                {
                    var lookup = luceneSuggesterDefinition.BuildSuggester(_fieldValueTypeCollection, _readerManager, rebuild);
                    if (_suggesters.ContainsKey(suggesterDefintion.Name))
                    {
                        _suggesters[suggesterDefintion.Name] = lookup;
                    }
                    else
                    {
                        _suggesters.Add(suggesterDefintion.Name, lookup);
                    }
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
    }
}
