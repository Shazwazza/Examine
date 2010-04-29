using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UmbracoExamine.Config;

namespace Examine.Test
{
    public class IndexInit
    {

        private static DirectoryInfo GetTemplateFolder() {
            var template = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.GetDirectories("TestIndex")
                .Single()
                .GetDirectories("TemplateIndex")
                .Single();
            return template;
        }

        private static DirectoryInfo GetWorkingFolder()
        {
            return new DirectoryInfo(Path.Combine(GetTemplateFolder().FullName, "..\\WorkingIndex"));
        }

        public static void RemoveWorkingIndex() 
        {
            var working = GetWorkingFolder();
            working.GetFiles().ToList().ForEach(x => x.Delete());
            working.Delete(true);
        }

        public static DirectoryInfo CreateFromTemplate()
        {           
            var template = GetTemplateFolder();
            var working = GetWorkingFolder();
            if (working.Exists) 
                working.Delete(true);
            working.Create();
            var indexDir = working.CreateSubdirectory("Index");
            template.GetFiles().ToList().ForEach(x => x.CopyTo(Path.Combine(indexDir.FullName, x.Name)));

            return working;
        }

        public static void UpdateIndexPaths()
        {
            ExamineLuceneIndexes.Instance.Sets.Cast<IndexSet>().ToList()
                .ForEach(x =>
                {
                    x.IndexPath = GetWorkingFolder().FullName;
                });
        }

    }
}
