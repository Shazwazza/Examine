using System;
using Examine.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Lucene.NET specific query options
    /// </summary>
    public class LuceneQueryOptions : QueryOptions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="skip">Number of result documents to skip.</param>
        /// <param name="take">Optional number of result documents to take.</param>
        /// <param name="searchAfter">Optionally skip to results after the results from the previous search execution. Used for efficent deep paging.</param>
        /// <param name="trackDocumentMaxScore">Whether to track the maximum document score. For best performance, if not needed, leave false.</param>
        /// <param name="trackDocumentScores">Whether to Track Document Scores. For best performance, if not needed, leave false.</param>
        /// <param name="facetSampling">Whether to apply Facet sampling to improve performance. If not required, leave null</param>
        public LuceneQueryOptions(int skip, int? take = null, SearchAfterOptions? searchAfter = null, bool trackDocumentScores = false, bool trackDocumentMaxScore = false, LuceneFacetSamplingQueryOptions? facetSampling = null)
            : base(skip, take)
        {
            TrackDocumentScores = trackDocumentScores;
            TrackDocumentMaxScore = trackDocumentMaxScore;
            SearchAfter = searchAfter;
            FacetRandomSampling = facetSampling;
        }

        /// <summary>
        /// Whether to Track Document Scores. For best performance, if not needed, leave false.
        /// </summary>
        public bool TrackDocumentScores { get; }

        /// <summary>
        /// Whether to track the maximum document score. For best performance, if not needed, leave false.
        /// </summary>
        public bool TrackDocumentMaxScore { get; }

        /// <summary>
        /// Options for Searching After. Used for efficent deep paging.
        /// </summary>
        public SearchAfterOptions? SearchAfter { get; }

        /// <summary>
        /// Options for Lucene Facet Sampling. If not set, no Facet Sampling is applied. 
        /// </summary>
        /// <remarks>
        /// Performance optimization for large sets
        /// </remarks>
        public LuceneFacetSamplingQueryOptions? FacetRandomSampling { get; }
    }
}
