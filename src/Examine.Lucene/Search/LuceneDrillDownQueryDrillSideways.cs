using Examine.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Lucene Drill Sideways
    /// </summary>
    public class LuceneDrillDownQueryDrillSideways : IDrillSideways
    {
        /// <summary>
        /// Top N Documents
        /// </summary>
        public int TopN { get; private set; } = 10;

        /// <inheritdoc/>
        public IDrillSideways SetTopN(int topN)
        {
            TopN = topN;
            return this;
        }
    }
}
