using System.IO;

namespace Examine
{
    /// <summary>
    /// Application Root
    /// </summary>
    public interface IApplicationRoot
    {
        /// <summary>
        /// Application Root Directory
        /// </summary>
        DirectoryInfo ApplicationRoot { get; }
    }
}
