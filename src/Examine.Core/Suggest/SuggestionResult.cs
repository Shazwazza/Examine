namespace Examine.Suggest
{
    public class SuggestionResult : ISuggestionResult
    {

        public SuggestionResult(string text, float? weight = null, int? frequency = null)
        {
            Text = text;
            Weight = weight;
            Frequency = frequency;
        }

        public string Text { get; }

        public float? Weight { get;  }

        public int? Frequency { get; }
    }
}
