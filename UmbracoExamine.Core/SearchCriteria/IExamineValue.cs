
namespace UmbracoExamine.Core.SearchCriteria
{
    public interface IExamineValue
    {
        Examineness Examineness { get; }
        string Value { get; }
    }
}
