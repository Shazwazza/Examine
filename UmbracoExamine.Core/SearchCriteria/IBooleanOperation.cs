
namespace UmbracoExamine.Core.SearchCriteria
{
    public interface IBooleanOperation
    {
        IQuery And();
        IQuery Or();
        IQuery Not();

        ISearchCriteria Compile();
    }
}
