using Examine.Lucene.Search;

namespace Examine.Search
{
    public interface IFacetFullTextField : IFacetField
    {
        /// <summary>
        /// Maximum number of terms to return
        /// </summary>
        int MaxCount { get; }

        /// <summary>
        /// Filter values
        /// </summary>
        string[] Values { get; }
    }
}
