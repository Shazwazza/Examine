using BenchmarkDotNet.Attributes;

namespace Examine.Benchmarks
{
    /// <summary>
    /// Measures allocation cost of constructing a ValueSet from an IDictionary&lt;string, object&gt;.
    /// The optimized source path eliminates the intermediate dictionary and N generator
    /// state machines that the historical NuGet versions allocate.
    /// </summary>
    [Config(typeof(NugetConfig))]
    [HideColumns("Arguments", "StdDev", "Error", "NuGetReferences")]
    [MemoryDiagnoser]
    public class ValueSetCtorBenchmarks
    {
        // Reusable input dictionary — represents a typical single-valued document field map
        private static readonly Dictionary<string, object> s_fiveFields = new()
        {
            { "title", "Sample Title" },
            { "body", "Sample body text for the document being indexed" },
            { "author", "Author Name" },
            { "category", "news" },
            { "date", "2024-01-15" }
        };

        // Larger document — more fields = more pronounced allocation savings per call
        private static readonly Dictionary<string, object> s_twentyFields = Enumerable
            .Range(1, 20)
            .ToDictionary(i => "field" + i, i => (object)("value" + i));

        /// <summary>
        /// Construct a ValueSet from a typical 5-field document dictionary.
        /// </summary>
        [Benchmark]
        public ValueSet FromDictionary5Fields()
            => new ValueSet("1", "content", s_fiveFields);

        /// <summary>
        /// Construct a ValueSet from a larger 20-field document dictionary.
        /// More fields = higher allocation savings per call.
        /// </summary>
        [Benchmark]
        public ValueSet FromDictionary20Fields()
            => new ValueSet("1", "content", s_twentyFields);

        /// <summary>
        /// Same as FromDictionary5Fields but with an explicit itemType parameter
        /// (the four-argument overload that PR #469 also optimizes).
        /// </summary>
        [Benchmark]
        public ValueSet FromDictionary5FieldsWithItemType()
            => new ValueSet("1", "content", "article", s_fiveFields);
    }
}
