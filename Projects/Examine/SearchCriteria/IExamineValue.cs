
namespace Examine.SearchCriteria
{
    public interface IExamineValue
    {
        Examineness Examineness { get; }
        float Level { get; }
        string Value { get; }
    }
}
