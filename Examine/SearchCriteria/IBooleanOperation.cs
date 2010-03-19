
namespace Examine.SearchCriteria
{
    public interface IBooleanOperation
    {
        IQuery And();
        IQuery Or();
        IQuery Not();

        ISearchCriteria Compile();
    }
}
