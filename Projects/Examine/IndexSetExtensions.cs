using System.Web;
using Examine.LuceneEngine.Config;

namespace Examine
{
    internal static class IndexSetExtensions
    {
        /// <summary>
        /// Used to replace any available tokens in the index path before the lucene directory is assigned to the path
        /// </summary>
        /// <param name="indexSet"></param>
        internal static void ReplaceTokensInIndexPath(this IndexSet indexSet)
        {
            if (indexSet == null) return;
            indexSet.IndexPath = indexSet.IndexPath
                .Replace("{machinename}", NetworkHelper.FileSafeMachineName)
                .Replace("{appdomainappid}", (HttpRuntime.AppDomainAppId ?? string.Empty).ReplaceNonAlphanumericChars(""))
                .EnsureEndsWith('/');
        }
    }
}