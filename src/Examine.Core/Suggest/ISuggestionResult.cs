namespace Examine.Suggest
{
    /// <summary>
    /// Suggestion Result
    /// </summary>
    public interface ISuggestionResult
    {
        /// <summary>
        /// Suggestion Text
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Suggestion Weight
        /// </summary>
        float? Weight { get; }

        /// <summary>
        /// Frequency of Suggestion text occurance
        /// </summary>
        int? Frequency { get; }
    }
}
