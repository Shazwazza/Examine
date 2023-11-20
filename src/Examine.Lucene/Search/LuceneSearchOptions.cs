using System;
using System.Globalization;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Options to configure <see cref="LuceneSearchQuery"/>
    /// </summary>
    public class LuceneSearchOptions
    {
        /// <summary>
        /// Whether terms of multi-term queries (e.g., wildcard, prefix, fuzzy and range)
        /// should be automatically lower-cased or not. Default is true.
        /// </summary>
        public bool? LowercaseExpandedTerms { get; set; }

        /// <summary>
        /// Set to true to allow leading wildcard characters.
        /// When set, * or ? are allowed as the first character of a Lucene.Net.Search.PrefixQuery
        /// and Lucene.Net.Search.WildcardQuery. Note that this can produce very slow queries
        /// on big indexes.
        /// Default: false.
        /// </summary>
        public bool? AllowLeadingWildcard { get; set; }

        /// <summary>
        /// Set to true to enable position increments in result query.
        /// When set, result phrase and multi-phrase queries will be aware of position increments.
        /// Useful when e.g. a Lucene.Net.Analysis.Core.StopFilter increases the position
        /// increment of the token that follows an omitted token.
        /// Default: false.
        /// </summary>
        public bool? EnablePositionIncrements { get; set; }

        /// <summary>
        /// By default, it uses Lucene.Net.Search.MultiTermQuery.CONSTANT_SCORE_AUTO_REWRITE_DEFAULT
        /// when creating a prefix, wildcard and range queries. This implementation is generally
        /// preferable because it a) Runs faster b) Does not have the scarcity of terms unduly
        /// influence score c) avoids any exception due to too many listeners. However, if
        /// your application really needs to use the old-fashioned boolean queries expansion
        /// rewriting and the above points are not relevant then use this change the rewrite
        /// method.
        /// </summary>
        public MultiTermQuery.RewriteMethod? MultiTermRewriteMethod { get; set; }

        /// <summary>
        /// Get or Set the prefix length for fuzzy queries. Default is 0.
        /// </summary>
        public int? FuzzyPrefixLength { get; set; }

        /// <summary>
        /// Get or Set locale used by date range parsing.
        /// </summary>
        public CultureInfo? Locale { get; set; }

        /// <summary>
        /// Gets or Sets the time zone.
        /// </summary>
        public TimeZoneInfo? TimeZone { get; set; }

        /// <summary>
        /// Gets or Sets the default slop for phrases. If zero, then exact phrase matches
        /// are required. Default value is zero.
        /// </summary>
        public int? PhraseSlop { get; set; }

        /// <summary>
        /// Get the minimal similarity for fuzzy queries.
        /// </summary>
        public float? FuzzyMinSim { get; set; }

        /// <summary>
        /// Sets the default Lucene.Net.Documents.DateTools.Resolution used for certain field
        /// when no Lucene.Net.Documents.DateTools.Resolution is defined for this field.
        /// </summary>
        public DateResolution? DateResolution { get; set; }

        /// <summary>
        /// Similarity Name
        /// </summary>
        public string? SimilarityName { get; set; }
    }
}
