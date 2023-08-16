using System;
using System.IO;

namespace Examine
{
    /// <inheritdoc/>
    public class CurrentEnvironmentApplicationRoot : IApplicationRoot
    {
        /// <inheritdoc/>
        public DirectoryInfo ApplicationRoot { get; } = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Examine"));
    }
}
