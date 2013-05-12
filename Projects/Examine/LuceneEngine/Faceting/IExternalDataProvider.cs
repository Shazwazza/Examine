namespace Examine.LuceneEngine.Faceting
{
    public interface IExternalDataProvider
    {
        object GetData(long id);
    }
}
