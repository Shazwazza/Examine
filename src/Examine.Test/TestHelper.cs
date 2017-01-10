using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Lucene.Net.Store;

namespace Examine.Test
{
    public class CustomRAMDirectory : RAMDirectory
    {
        private readonly string _lockId = Guid.NewGuid().ToString();
        public override string GetLockID()
        {
            return _lockId;
        }
    }

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