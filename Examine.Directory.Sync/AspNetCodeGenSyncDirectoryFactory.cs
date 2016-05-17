using System;
using System.IO;
using System.Web;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Store;

namespace Examine.Directory.Sync
{
    /// <summary>
    /// A directory factory used to create an instance of SyncDirectory that uses AspNet codegen as the cache directory
    /// </summary>
    public class AspNetCodeGenSyncDirectoryFactory : IDirectoryFactory
    {
        public Lucene.Net.Store.Directory CreateDirectory(LuceneIndexer indexer, string luceneIndexFolder)
        {
            var indexFolder = new DirectoryInfo(luceneIndexFolder);
            var codeGen = GetLocalStorageDirectory(indexFolder);
            var master = new DirectoryInfo(luceneIndexFolder);
            return new SyncDirectory(new SimpleFSDirectory(master), new SimpleFSDirectory(codeGen));
        }

        private DirectoryInfo GetLocalStorageDirectory(DirectoryInfo indexPath)
        {
            var codegenPath = HttpRuntime.CodegenDir;
            var path = Path.Combine(codegenPath, "App_Data", "TEMP", "ExamineIndexes", indexPath.Name);
            return new DirectoryInfo(path);
        }
    }
}