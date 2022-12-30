namespace Examine.Lucene.Search
{
    /// <summary>
    /// Options for Lucene Facet Sampling
    /// </summary>
    public class LuceneFacetSamplingQueryOptions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sampleSize"> The preferred sample size. If the number of hits is greater than
        ///          the size, sampling will be done using a sample ratio of sampling
        ///          size / totalN. For example: 1000 hits, sample size = 10 results in
        ///          samplingRatio of 0.01. If the number of hits is lower, no sampling
        ///          is done at all</param>
        /// <param name="seed">The random seed. If 0 then a seed will be chosen for you.</param>
        public LuceneFacetSamplingQueryOptions(int sampleSize, long seed)
        {
            SampleSize = sampleSize;
            Seed = seed;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sampleSize"> The preferred sample size. If the number of hits is greater than
        ///          the size, sampling will be done using a sample ratio of sampling
        ///          size / totalN. For example: 1000 hits, sample size = 10 results in
        ///          samplingRatio of 0.01. If the number of hits is lower, no sampling
        ///          is done at all</param>
        public LuceneFacetSamplingQueryOptions(int sampleSize)
        {
            SampleSize = sampleSize;
            Seed = 0;
        }

        /// <summary>
        /// The preferred sample size. If the number of hits is greater than
        ///          the size, sampling will be done using a sample ratio of sampling
        ///          size / totalN. For example: 1000 hits, sample size = 10 results in
        ///          samplingRatio of 0.01. If the number of hits is lower, no sampling
        ///          is done at all
        /// </summary>
        public int SampleSize { get; }

        /// <summary>
        /// The random seed. If 0 then a seed will be chosen for you.
        /// </summary>
        public long Seed { get; }

    }
}
