using System;
using Lucene.Net.Analysis;
using Lucene.Net.Facet;
using Microsoft.Extensions.Options;

namespace Examine.Lucene.Providers
{
    internal sealed class SearcherOptions : IOptionsMonitor<LuceneSearcherOptions>
    {
        private readonly LuceneSearcherOptions _options;

        public SearcherOptions(Analyzer analyzer, FacetsConfig facetsConfig)
        {
            _options = new LuceneSearcherOptions
            {
                Analyzer = analyzer,
                FacetConfiguration = facetsConfig
            };
        }

        public LuceneSearcherOptions Get(string? name) => _options;

        public LuceneSearcherOptions CurrentValue => throw new NotSupportedException();

        public IDisposable OnChange(Action<LuceneSearcherOptions, string> listener) => throw new NotSupportedException();
    }
}

