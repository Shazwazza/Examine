using System;
using System.IO;
using System.Reflection;

namespace Examine.Test
{
    public static class TestHelper
    {
        static public string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string MapPathForTest(string relativePath)
        {
            if (relativePath == null) throw new ArgumentNullException("relativePath");

            return relativePath.Replace("~/", AssemblyDirectory + "/");
        }
    }
}