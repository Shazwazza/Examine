using System;
using System.IO;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.LuceneEngine.Directories
{
    public abstract class DirectoryFactory : IDirectoryFactory
    {
        static DirectoryFactory()
        {
            DefaultLockFactory = d =>
            {
                var nativeFsLockFactory = new NativeFSLockFactory(d);
                nativeFsLockFactory.LockPrefix = null;
                return nativeFsLockFactory;
            };
        }

        /// <summary>
        /// This can be changed on startup to use a different lock factory than the default which is <see cref="NativeFSLockFactory"/>
        /// </summary>
        public static Func<DirectoryInfo, LockFactory> DefaultLockFactory { get; set; }

        public abstract Directory CreateDirectory(DirectoryInfo luceneIndexFolder);
   
    }
}