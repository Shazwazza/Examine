using System;
using System.IO;
using System.Web;

namespace Examine
{
    // ReSharper disable once InconsistentNaming
    internal class IOHelper
    {
        /// <summary>
        /// Returns the machine name that is safe to use in file paths.
        /// </summary>
        private static string FileSafeMachineName => MachineName.ReplaceNonAlphanumericChars("-");

        /// <summary>
        /// Returns the current machine name
        /// </summary>
        /// <remarks>
        /// Tries to resolve the machine name, if it cannot it uses the config section.
        /// </remarks>
        private static string MachineName
        {
            get
            {
                try
                {
                    return Environment.MachineName;
                }
                catch
                {
                    try
                    {
                        return System.Net.Dns.GetHostName();
                    }
                    catch
                    {
                        //if we get here it means we cannot access the machine name
                        throw new ApplicationException("Cannot resolve the current machine name eithe by Environment.MachineName or by Dns.GetHostname()");
                    }
                }
            }
        }

        /// <summary>
        /// Used to replace any available tokens in the index path before the lucene directory is assigned to the path
        /// </summary>
        /// <param name="path"></param>
        internal static string ReplaceTokensInIndexPath(string path)
        {
            return path?.Replace("{machinename}", FileSafeMachineName)
                //todo: Find replacement for HttpRuntime.AppDomainAppId 
                .Replace("{appdomainappid}", ( string.Empty).ReplaceNonAlphanumericChars(""))
                .EnsureEndsWith('/');
        }

        public static string MapPath(string configPath)
        {
            if (!configPath.StartsWith("~/")) return configPath;

            // Support unit testing scenario where hosting environment is not initialized.
            //todo find replacement fo = HostingEnvironment.IsHosted   HostingEnvironment.MapPath("~/")
              
            var hostingRoot = AppDomain.CurrentDomain.BaseDirectory;

            return Path.Combine(hostingRoot, configPath.Substring(2).Replace('/', '\\'));
        }
    }
}