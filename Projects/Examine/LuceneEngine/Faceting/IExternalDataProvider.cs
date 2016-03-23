namespace Examine.LuceneEngine.Faceting
{
    //TODO: We need to investigate this
    internal interface IExternalDataProvider
    {
        object GetData(long id);
    }
}
