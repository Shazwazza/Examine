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
        /// <param name="sampleSize">The preferred sample size. If the number of hits is greater than the size, sampling will be done using a sample ratio of sample size / total hit count.</param>
        /// <param name="seed">The random seed. If 0 then a seed will be chosen for you.</param>
        public LuceneFacetSamplingQueryOptions(int sampleSize, long seed)
        {
            SampleSize = sampleSize;
            Seed = seed;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sampleSize">The preferred sample size. If the number of hits is greater than the size, sampling will be done using a sample ratio of sample size / total hit count.</param>
        public LuceneFacetSamplingQueryOptions(int sampleSize)
        {
            SampleSize = sampleSize;
            Seed = 0;
        }

        /// <summary>
        /// The preferred sample size. If the number of hits is greater than the size, sampling will be done using a sample ratio of sample size / total hit count.
        /// </summary>
        public int SampleSize { get; }

        /// <summary>
        /// The random seed. If 0 then a seed will be chosen for you.
        /// </summary>
        public long Seed { get; }
    }
}
