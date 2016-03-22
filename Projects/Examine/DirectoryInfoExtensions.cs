using System.IO;
using System.Web;

namespace Examine
{
    internal static class DirectoryInfoExtensions
    {
        /// <summary>
        /// Used to replace any available tokens in the index path before the lucene directory is assigned to the path
        /// </summary>
        /// <param name="dir"></param>
        internal static DirectoryInfo ReplaceTokensInPath(this DirectoryInfo dir)
        {
            if (dir == null) return null;

            var fullPath = dir.FullName;

            fullPath = fullPath
                .Replace("{machinename}", NetworkHelper.FileSafeMachineName)
                .Replace("{appdomainappid}", (HttpRuntime.AppDomainAppId ?? string.Empty).ReplaceNonAlphanumericChars(""));

            return new DirectoryInfo(fullPath);
        }
    }
}