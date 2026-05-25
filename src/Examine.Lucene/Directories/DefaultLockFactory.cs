using System.IO;
using Lucene.Net.Store;

namespace Examine.Lucene.Directories
{
    /// <inheritdoc/>
    public class DefaultLockFactory : ILockFactory
    {
        /// <inheritdoc/>
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
