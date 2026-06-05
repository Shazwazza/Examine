using System.IO;
using Lucene.Net.Store;

namespace Examine.Lucene.Directories
{
    /// <summary>
    /// A factory for creating lock files
    /// </summary>
    public interface ILockFactory
    {
        /// <summary>
        /// Gets the lock factory
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        LockFactory GetLockFactory(DirectoryInfo directory);
    }
}
