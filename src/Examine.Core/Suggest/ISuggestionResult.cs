namespace Examine.Suggest
{
    public interface ISuggestionResult
    {
        string Text { get; }

        long Weight { get; }
    }
}
