using System.IO;
using Lucene.Net.Store;

namespace Examine.Lucene.Directories
{
    public interface ILockFactory
    {
        LockFactory GetLockFactory(DirectoryInfo directory);
    }
}
