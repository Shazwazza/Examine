using Lucene.Net.Store;

namespace Examine.Lucene.Suggest.Directories
{
    /// <summary>
    /// RAMDirectory Suggester Directory Factory
    /// </summary>
    public class RAMSuggesterDirectoryFactory : SuggesterDirectoryFactoryBase
    {
        /// <inheritdoc/>
        protected override Directory CreateDirectory(string name, bool forceUnlock) => new RAMDirectory();
    }
}
