using System.IO;
using System.Security;
using System.Web;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Directories
{
    /// <summary>
    /// A directory factory used to create an instance of SyncDirectory that uses AspNet codegen as the cache directory
    /// </summary>
    public class SyncAspNetCodeGenDirectoryFactory : DirectoryFactory
    {
        
        public override Lucene.Net.Store.Directory CreateDirectory(DirectoryInfo luceneIndexFolder)
        {
            var master = luceneIndexFolder;
            var codeGen = GetLocalStorageDirectory(master);
            var masterDir = new SimpleFSDirectory(master);
            var cacheDir = new SimpleFSDirectory(codeGen);            
            masterDir.SetLockFactory(DefaultLockFactory(master));
            cacheDir.SetLockFactory(DefaultLockFactory(codeGen));
            return new SyncDirectory(masterDir, cacheDir);
        }

        private DirectoryInfo GetLocalStorageDirectory(DirectoryInfo indexPath)
        {
            var codegenPath = HttpRuntime.CodegenDir;
            var path = Path.Combine(codegenPath, "App_Data", "TEMP", "ExamineIndexes", indexPath.Name);
            return new DirectoryInfo(path);
        }
    }
}