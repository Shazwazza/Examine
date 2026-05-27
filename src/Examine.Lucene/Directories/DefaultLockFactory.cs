using System.IO;
using Lucene.Net.Store;

namespace Examine.Lucene.Directories
{
    public class DefaultLockFactory : ILockFactory
    {
        public LockFactory GetLockFactory(DirectoryInfo directory)
        {
            var nativeFsLockFactory = new NativeFSLockFactory(directory)
            {
                LockPrefix = null
            };
            return nativeFsLockFactory;
        }
    }
}
