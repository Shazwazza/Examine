using System.IO;

namespace Examine
{
    public interface IApplicationRoot
    {
        DirectoryInfo ApplicationRoot { get; }
    }
}
