using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Examine.Test
{
    public static class TestHelper
    {
        public static string AssemblyDirectory
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

		public static void CleanupFolder(DirectoryInfo d)
		{
			foreach (var f in d.GetDirectories())
			{
				try
				{
					f.Delete(true);
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Could not remove folder" + ex.Message);
				}
			}
		}
    }
}