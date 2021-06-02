using System;
using System.IO;

namespace Examine
{
    public class CurrentEnvironmentApplicationRoot : IApplicationRoot
    {
        public DirectoryInfo ApplicationRoot { get; } = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Examine"));
    }
}
