namespace Examine.LuceneEngine.Facets
{
    public interface IExternalDataProvider
    {
        object GetData(long id);
    }
}
