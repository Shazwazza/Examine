using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UmbracoExamine.Providers
{
    public static class FileSystemExtensions
    {

        /// <summary>
        /// Deletes all files in the folder and returns the number deleted.
        /// </summary>
        /// <param name="di"></param>
        /// <returns></returns>
        public static int ClearFiles(this DirectoryInfo di)
        {
            int count = 0;
            if (di.Exists)
            {
                di.GetFiles().ToList()
                .ForEach(x =>
                {
                    x.Delete();
                    count++;
                });
            }
            return count;
        }

    }
}
