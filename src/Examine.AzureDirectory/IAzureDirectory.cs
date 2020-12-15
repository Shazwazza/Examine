namespace Examine.AzureDirectory
{
    public interface IAzureDirectory
    {
        bool ShouldCompressFile(string path);
    }
}