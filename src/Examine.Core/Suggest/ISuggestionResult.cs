namespace Examine.Suggest
{
    public interface ISuggestionResult
    {
        string Text { get; }

        float? Weight { get; }

        int? Frequency { get; }
    }
}
