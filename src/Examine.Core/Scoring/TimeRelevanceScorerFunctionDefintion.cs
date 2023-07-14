using System;

namespace Examine.Scoring
{
    /// <summary>
    /// Boosts relevance based on time recency
    /// </summary>
    public class TimeRelevanceScorerFunctionDefintion : RelevanceScorerFunctionBaseDefintion
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="boost">Boost</param>
        /// <param name="boostTimeRange">Duration from current time to boost from <see cref="ExamineClock.CurrentTime"/></param>
        public TimeRelevanceScorerFunctionDefintion(string fieldName, float boost, TimeSpan boostTimeRange) : base(fieldName, boost)
        {
            BoostTimeRange = boostTimeRange;
        }

        /// <summary>
        /// Time range to boost from
        /// </summary>
        public TimeSpan BoostTimeRange { get; }
    }
}
