namespace Examine.Search
{
    /// <summary>
    /// Provides extension methods for the Examineness enumeration to enhance its functionality.
    /// </summary>
    public static class ExamineValueExtensions
    {
        /// <summary>
        /// Applies the Boosted flag to the specified Examine value.
        /// </summary>
        public static IExamineValue WithBoost(this IExamineValue examineValue, float boost)
            => ExamineValue.WithBoost(examineValue, boost);
    }
}
