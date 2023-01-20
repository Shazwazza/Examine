using Lucene.Net.Store;

namespace Examine.Lucene.Suggest.Directories
{
    public class RAMSuggesterDirectoryFactory : SuggesterDirectoryFactoryBase
    {
        protected override Directory CreateDirectory(string name, bool forceUnlock) => new RAMDirectory();
    }
}
