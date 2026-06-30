using BenchmarkDotNet.Attributes;

namespace Examine.Benchmarks
{
    /// <summary>
    /// Measures allocation cost of constructing a ValueSet from an IDictionary&lt;string, object&gt;.
    /// The optimized source path eliminates the intermediate dictionary and N generator
    /// state machines that the historical NuGet versions allocate.
    /// </summary>
    /// <remarks>
    /// Results (BenchmarkDotNet v0.14.0, .NET 8.0.28, AMD EPYC 9V74 2 physical cores,
    /// ShortRun: 3 warmup / 3 iterations / 1 launch). "Source" is the current support/3.x
    /// build; 3.0.1–3.3.0 are the published NuGet packages.
    ///
    /// | Method                            | Job    | Mean       | Allocated |
    /// |---------------------------------- |------- |-----------:|----------:|
    /// | FromDictionary5Fields             | 3.3.0  | 1,183.5 ns |    2200 B |
    /// | FromDictionary20Fields            | 3.3.0  | 4,007.1 ns |    6544 B |
    /// | FromDictionary5FieldsWithItemType | 3.3.0  | 1,179.0 ns |    2200 B |
    /// | FromDictionary5Fields             | Source |   225.6 ns |     592 B |
    /// | FromDictionary20Fields            | Source |   653.7 ns |    1520 B |
    /// | FromDictionary5FieldsWithItemType | Source |   227.8 ns |     592 B |
    ///
    /// The 3.0.1/3.1.0/3.2.1 packages match 3.3.0 within noise. The source build drops the
    /// 5-field constructors from 2200 B to 592 B (~3.7x less) and the 20-field constructor
    /// from 6544 B to 1520 B (~4.3x less), with a ~5–6x throughput gain, by removing the
    /// intermediate dictionary and the per-field generator state machines.
    /// </remarks>
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
