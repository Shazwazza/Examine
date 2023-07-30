namespace Examine.Suggest
{
    /// <summary>
    /// Suggestion Result
    /// </summary>
    public class SuggestionResult : ISuggestionResult
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">Suggestion Text</param>
        /// <param name="weight">Suggestion Weight</param>
        /// <param name="frequency">Suggestion Text frequency</param>
        public SuggestionResult(string text, float? weight = null, int? frequency = null)
        {
            Text = text;
            Weight = weight;
            Frequency = frequency;
        }

        /// <inheritdoc/>
        public string Text { get; }

        /// <inheritdoc/>
        public float? Weight { get;  }

        /// <inheritdoc/>
        public int? Frequency { get; }
    }
}
