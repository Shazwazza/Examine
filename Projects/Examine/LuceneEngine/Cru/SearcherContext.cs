using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
//using Examine.LuceneEngine.Facets;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Indexing.ValueTypes;
using Lucene.Net.Analysis;
using Lucene.Net.Contrib.Management;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace LuceneManager.Infrastructure
{
    public class SearcherContext : IDisposable
    {
        public Directory Directory { get; private set; }


        public NrtManager Manager { get; private set; }

        public PerFieldAnalyzerWrapper Analyzer { get; private set; }

        public FacetsLoader FacetsLoader { get; private set; }

        private readonly IndexWriter _writer;

        private readonly NrtManagerReopener _reopener;
        public Committer Committer { get; private set; }

        private readonly List<Thread> _threads = new List<Thread>();

        public SearcherContext(Directory dir, Analyzer defaultAnalyzer, FacetConfiguration facetConfiguration = null)
            : this(dir, defaultAnalyzer, TimeSpan.FromMilliseconds(25), TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(10), TimeSpan.FromHours(2), facetConfiguration)
        {
        }

        public SearcherContext(Directory dir, Analyzer defaultAnalyzer,
                        TimeSpan targetMinStale, TimeSpan targetMaxStale,
                        TimeSpan commitInterval, TimeSpan optimizeInterval
                        , FacetConfiguration facetConfiguration = null)
        {
            Directory = dir;
            Analyzer = new PerFieldAnalyzerWrapper(defaultAnalyzer);


            FacetsLoader = new FacetsLoader(facetConfiguration);


            _writer = new IndexWriter(dir, Analyzer, IndexWriter.MaxFieldLength.UNLIMITED);


            Manager = new NrtManager(_writer, FacetsLoader);
            _reopener = new NrtManagerReopener(Manager, targetMaxStale, targetMinStale);
            Committer = new Committer(_writer, commitInterval, optimizeInterval);

            _threads.AddRange(new[] { new Thread(_reopener.Start), new Thread(Committer.Start) });

            using (var s = GetSearcher())
            {
                FacetsLoader.Warm(s.Searcher);
            }

            foreach (var t in _threads)
            {
                t.Start();
            }
        }

        public Func<string, IIndexValueType> DefaultValueTypeFactory = name => new RawStringType(name);
        private ConcurrentDictionary<string, IIndexValueType> _valueTypes = new ConcurrentDictionary<string, IIndexValueType>(StringComparer.InvariantCultureIgnoreCase);

        public IIndexValueType GetValueType(string fieldName, bool useDefaultIfMissing = false)
        {
            if (useDefaultIfMissing)
            {
                return _valueTypes.GetOrAdd(fieldName, n =>
                    {
                        var t = DefaultValueTypeFactory(n);
                        t.SetupAnalyzers(Analyzer);
                        return t;
                    });
            }
            IIndexValueType type;
            return _valueTypes.TryGetValue(fieldName, out type) ? type : null;
        }

        public void DefineValueType(IIndexValueType type)
        {
            if (_valueTypes.TryAdd(type.FieldName, type))
            {
                type.SetupAnalyzers(Analyzer);
            }
        }

        public SearcherManager.IndexSearcherToken GetSearcher()
        {
            return Manager.GetSearcherManager().Acquire();
        }

        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;


            var disposeActions = new List<Action>
                {
                    FacetsLoader.Dispose,
                    _reopener.Dispose,
                    Committer.Dispose,
                    Manager.Dispose,
                    () => _writer.Close(true)
                };

            disposeActions.AddRange(_threads.Select(t => (Action)t.Join));

            DisposeUtil.PostponeExceptions(disposeActions.ToArray());
        }
    }
}