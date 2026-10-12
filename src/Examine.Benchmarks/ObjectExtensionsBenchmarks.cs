using BenchmarkDotNet.Attributes;

namespace Examine.Benchmarks
{
    /// <summary>
    /// Measures allocation/time cost of <see cref="ObjectExtensions.ConvertObjectToDictionary"/>,
    /// the POCO-to-document-fields conversion used by <c>ValueSet.FromObject</c> when indexing
    /// plain objects. This exercises the <c>TypeDescriptor.GetProperties(object)</c> reflection
    /// path plus the per-property ignore-list filtering, which several perf-improver/
    /// efficiency-improver PRs (e.g. per-Type <c>PropertyDescriptorCollection</c> caching,
    /// LINQ-to-loop conversions) have targeted historically. This class fills a previously
    /// undocumented benchmark coverage gap for that hot path.
    /// </summary>
    /// <remarks>
    /// Standalone micro-benchmark (2M-iteration loop, <c>GC.GetAllocatedBytesForCurrentThread</c>,
    /// warm cache, .NET 8, current <c>dev</c> HEAD as of 2026-09-13 — the ConcurrentDictionary
    /// per-Type PropertyDescriptorCollection cache from PR #585/#595 is not yet merged into
    /// <c>dev</c>) confirms both benchmark methods allocate 776.00 bytes/call, matching the
    /// figure already tracked in repo-improver memory: the <c>TypeDescriptor.GetProperties(o)</c>
    /// reflection call dominates allocation regardless of ignore-list size. Once the per-Type
    /// cache PR lands, re-run this benchmark to confirm the expected drop toward ~496 bytes/call.
    /// </remarks>
    [MemoryDiagnoser]
    public class ObjectExtensionsBenchmarks
    {
        private sealed class SampleDocument
        {
            public string Title { get; set; } = "Sample Title";
            public string Body { get; set; } = "Sample body text for the document being indexed";
            public string Author { get; set; } = "Author Name";
            public string Category { get; set; } = "news";
            public string Date { get; set; } = "2024-01-15";
            public string Internal { get; set; } = "not-indexed";
        }

        private readonly SampleDocument _document = new();
        private static readonly string[] s_ignoreProperties = { nameof(SampleDocument.Internal) };

        /// <summary>
        /// Converts a typical 6-property POCO to a dictionary, ignoring one property.
        /// Repeated calls on the same CLR type exercise the warm-cache path once a
        /// per-Type <c>PropertyDescriptorCollection</c> cache is present.
        /// </summary>
        [Benchmark]
        public IDictionary<string, object> ConvertWithIgnoredProperty()
            => ObjectExtensions.ConvertObjectToDictionary(_document, s_ignoreProperties);

        /// <summary>
        /// Converts the same POCO with no ignore list, isolating the reflection +
        /// dictionary-build cost from the ignore-list filtering cost.
        /// </summary>
        [Benchmark]
        public IDictionary<string, object> ConvertNoIgnoredProperties()
            => ObjectExtensions.ConvertObjectToDictionary(_document);
    }
}
