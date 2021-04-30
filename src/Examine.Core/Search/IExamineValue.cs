
namespace Examine.Search
{
    public interface IExamineValue
    {
        Examineness Examineness { get; }
        float Level { get; }
        string Value { get; }
    }
}
