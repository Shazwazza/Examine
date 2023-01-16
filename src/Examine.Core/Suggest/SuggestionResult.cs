namespace Examine.Suggest
{
    public class SuggestionResult : ISuggestionResult
    {

        public SuggestionResult(string text, long weight)
        {
            Text = text;
            Weight = weight;
        }

        public string Text { get; }

        public long Weight { get;  }
    }
}
