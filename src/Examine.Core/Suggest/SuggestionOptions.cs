namespace Examine.Suggest
{
    /// <summary>
    /// Suggester Options
    /// </summary>
    public class SuggestionOptions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="top">Clamp number of results</param>
        /// <param name="suggesterName">The name of the Suggester to use</param>
        public SuggestionOptions(int top = 5, string suggesterName = null)
        {
            Top = top;
            if (top < 0)
            {
                top = 0;
            }
            SuggesterName = suggesterName;
        }

        /// <summary>
        /// Clamp number of results.
        /// </summary>
        public int Top { get; }

        /// <summary>
        /// The name of the Suggester to use
        /// </summary>
        public string SuggesterName { get; }
    }
}
