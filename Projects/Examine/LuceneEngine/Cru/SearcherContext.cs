//using Examine.LuceneEngine.Facets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Indexing.ValueTypes;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Cru
{
    /// <summary>
    /// A searcher context
    /// </summary>
    public class SearcherContext : IDisposable
    {
        /// <summary>
        /// Returns the current lucene directory
        /// </summary>
        public Directory Directory { get; private set; }
        
        /// <summary>
        /// Returns the near real time manager
        /// </summary>
        internal NrtManager Manager { get; private set; }

        /// <summary>
        /// Returns the PerFieldAnalyzerWrapper
        /// </summary>
        internal PerFieldAnalyzerWrapper Analyzer { get; private set; }

        /// <summary>
        /// Returns the FacetsLoader
        /// </summary>
        public FacetsLoader FacetsLoader { get; private set; }

        /// <summary>
        /// Returns the Lucene Index Writer
        /// </summary>
        /// <remarks>
        /// This is only exposed to allow for exposing on extension methods for backward compat reasons - however it should not be used.
        /// </remarks>
        internal IndexWriter Writer { get; private set; }
        
        private readonly NrtManagerReopener _reopener;
        internal Committer Committer { get; private set; }
        
        /// <summary>
        /// Used for debugging purposes
        /// </summary>
        internal Guid SearcherContextId { get; } = Guid.NewGuid();

        private readonly List<Thread> _threads = new List<Thread>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="defaultAnalyzer"></param>
        /// <param name="facetConfiguration"></param>
        public SearcherContext(Directory dir, Analyzer defaultAnalyzer, FacetConfiguration facetConfiguration = null)
            : this(dir, defaultAnalyzer, TimeSpan.FromMilliseconds(25), TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(10), TimeSpan.FromHours(2), facetConfiguration)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="defaultAnalyzer"></param>
        /// <param name="targetMinStale"></param>
        /// <param name="targetMaxStale"></param>
        /// <param name="commitInterval"></param>
        /// <param name="optimizeInterval"></param>
        /// <param name="facetConfiguration"></param>
        public SearcherContext(Directory dir, Analyzer defaultAnalyzer,
                        TimeSpan targetMinStale, TimeSpan targetMaxStale,
                        TimeSpan commitInterval, TimeSpan optimizeInterval, 
                        FacetConfiguration facetConfiguration = null)
        {
            Directory = dir;
            Analyzer = new PerFieldAnalyzerWrapper(defaultAnalyzer);


            FacetsLoader = new FacetsLoader(facetConfiguration);

            var warmer = new CompositeSearcherWarmer(new[] {(ISearcherWarmer) FacetsLoader, new ValueTypeWarmer(this)});

            Writer = new IndexWriter(dir, Analyzer, IndexWriter.MaxFieldLength.UNLIMITED);


            Manager = new NrtManager(Writer, warmer);
            _reopener = new NrtManagerReopener(Manager, targetMaxStale, targetMinStale);
            Committer = new Committer(Writer, commitInterval, optimizeInterval);

            _threads.AddRange(new[] { new Thread(_reopener.Start), new Thread(Committer.Start) });

            using (var s = GetSearcher())
            {
                warmer.Warm(s.Searcher);
            }

            foreach (var t in _threads)
            {                
                t.Start();
                Trace.WriteLine($"thread {t.ManagedThreadId} Started");
            }

            Trace.WriteLine($"SearcherContext {SearcherContextId} Created");
        }

        internal Func<string, IIndexValueType> DefaultValueTypeFactory = name => new FullTextType(name);
        internal ConcurrentDictionary<string, IIndexValueType> ValueTypes = new ConcurrentDictionary<string, IIndexValueType>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Returns the list of explitly registered value types
        /// </summary>
        public IEnumerable<IIndexValueType> RegisteredValueTypes
        {
            get { return ValueTypes.Values; }
        }

        /// <summary>
        /// Returns the value type for the field name specified, if useDefaultIfMissing then it will resolve the value
        /// type from the DefaultValueTypeFactory method if the field has not been registered explicitly
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="useDefaultIfMissing"></param>
        /// <returns></returns>
        public IIndexValueType GetValueType(string fieldName, bool useDefaultIfMissing = false)
        {
            if (useDefaultIfMissing)
            {
                return ValueTypes.GetOrAdd(fieldName, n =>
                    {
                        var t = DefaultValueTypeFactory(n);
                        t.SetupAnalyzers(Analyzer);
                        return t;
                    });
            }
            IIndexValueType type;
            return ValueTypes.TryGetValue(fieldName, out type) ? type : null;
        }

        /// <summary>
        /// Explicitly defines a value type and sets it up
        /// </summary>
        /// <param name="type"></param>
        public void DefineValueType(IIndexValueType type)
        {
            if (ValueTypes.TryAdd(type.FieldName, type))
            {
                type.SetupAnalyzers(Analyzer);
            }
        }

        /// <summary>
        /// Returns a new searcher (ensure it's disposed!)
        /// </summary>
        /// <returns></returns>
        internal SearcherManager.IndexSearcherToken GetSearcher()
        {
            return Manager.GetSearcherManager().Acquire();
        }

        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            //TODO: Should dispose the Lucene Directory ?

            Trace.WriteLine($"SearcherContext {SearcherContextId} Dispose");

            var disposeActions = new List<Action>
                {
                    FacetsLoader.Dispose,
                    _reopener.Dispose,
                    Committer.Dispose,
                    Manager.Dispose,
                    () =>
                    {
                        Trace.WriteLine("_writer.Dispose");
                        Writer.Dispose(true);
                    }
                };

            disposeActions.AddRange(_threads.Select(t =>
            {                
                Trace.WriteLine($"thread {t.ManagedThreadId} Dispose");
                return (Action) t.Join;
            }));

            DisposeUtil.PostponeExceptions(disposeActions.ToArray());
        }
    }
}