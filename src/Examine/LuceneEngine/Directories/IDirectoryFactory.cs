using System;
using System.IO;
using System.Security;
using Examine.Logging;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.LuceneEngine.Directories
{
    /// <summary>
    /// Creates a Lucene <see cref="Lucene.Net.Store.Directory"/> for an index
    /// </summary>
    public interface IDirectoryFactory
    {   

        Directory CreateDirectory(DirectoryInfo luceneIndexFolder);
   
    }
    public interface IDirectoryFactory2
    {   
        Directory CreateDirectory(DirectoryInfo luceneIndexFolder, ILoggingService loggingService);
   
    }
}