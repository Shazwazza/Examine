using System;
using Examine.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Lucene.NET specific query options
    /// </summary>
    public class LuceneQueryOptions : QueryOptions
    {
        // TODO: Review all this
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="skip">Number of result documents to skip.</param>
        /// <param name="take">Optional number of result documents to take.</param>
        /// <param name="searchAfter">Optionally skip to results after the results from the previous search execution. Used for efficient deep paging.</param>
        /// <param name="trackDocumentScores">Whether to Track Document Scores. For best performance, if not needed, leave false.</param>
        [Obsolete("To remove in Examine 5.0")]
#pragma warning disable RS0027 // API with optional parameter(s) should have the most parameters amongst its public overloads
        public LuceneQueryOptions(int skip, int? take = null, SearchAfterOptions? searchAfter = null, bool trackDocumentScores = false, bool trackDocumentMaxScore = false,
            int skipTakeMaxResults = AbsoluteMaxResults,
            bool autoCalculateSkipTakeMaxResults = false)
#pragma warning restore RS0027 // API with optional parameter(s) should have the most parameters amongst its public overloads
            : base(skip, take)
        {
            TrackDocumentScores = trackDocumentScores;
            TrackDocumentMaxScore = trackDocumentMaxScore;
            SearchAfter = searchAfter;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="facetSampling">Whether to apply Facet sampling to improve performance. If not required, leave null</param>
        /// <param name="skip">Number of result documents to skip.</param>
        /// <param name="take">Optional number of result documents to take.</param>
        /// <param name="searchAfter">Optionally skip to results after the results from the previous search execution. Used for efficent deep paging.</param>
        /// <param name="trackDocumentMaxScore">Whether to track the maximum document score. For best performance, if not needed, leave false.</param>
        /// <param name="trackDocumentScores">Whether to Track Document Scores. For best performance, if not needed, leave false.</param>
        public LuceneQueryOptions(LuceneFacetSamplingQueryOptions? facetSampling, int skip, int? take, SearchAfterOptions? searchAfter, bool trackDocumentScores, bool trackDocumentMaxScore)
            : base(skip, take)
        {
            SearchAfter = searchAfter;
            TrackDocumentScores = trackDocumentScores;
            TrackDocumentMaxScore = trackDocumentMaxScore;
            SkipTakeMaxResults = skipTakeMaxResults;
            FacetRandomSampling = facetSampling;
            AutoCalculateSkipTakeMaxResults = autoCalculateSkipTakeMaxResults;
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
        /// Options for Searching After. Used for efficient deep paging.
        /// </summary>
        public SearchAfterOptions? SearchAfter { get; }

        /// <summary>
        /// Options for Lucene Facet Sampling. If not set, no Facet Sampling is applied. 
        /// </summary>
        /// <remarks>
        /// Performance optimization for large sets
        /// </remarks>
        public LuceneFacetSamplingQueryOptions? FacetRandomSampling { get; }

        /// <summary>
        /// When using Skip/Take (not SearchAfter) this will be the maximum data set size that can be paged.
        /// </summary>
        /// <remarks>
        /// For performance reasons, this should be low.
        /// The default is 10k and if larger datasets are required to be paged,
        /// this value can be increased but it is recommended to use the SearchAfter feature instead.
        /// </remarks>
        public int SkipTakeMaxResults { get; }

        /// <summary>
        /// If enabled, this will pre-calculate the document count in the index to use for <see cref="SkipTakeMaxResults"/>.
        /// </summary>
        /// <remarks>
        /// This will incur a performance hit on each search execution since there will be a query to get the total document count.
        /// </remarks>
        public bool AutoCalculateSkipTakeMaxResults { get; }
    }
}
