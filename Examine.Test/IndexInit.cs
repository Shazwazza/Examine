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
        public IndexInit(string workingIndexFolderName)
        {
            m_WorkingIndexFolderName = workingIndexFolderName;
        }

        private string m_WorkingIndexFolderName;

        private DirectoryInfo GetTemplateFolder() {
            var template = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.GetDirectories("TestIndex")
                .Single()
                .GetDirectories("TemplateIndex")
                .Single();
            return template;
        }

        private DirectoryInfo GetWorkingFolder()
        {
            var di = new DirectoryInfo(Path.Combine(GetTemplateFolder().FullName, "..\\" + m_WorkingIndexFolderName));
            if (!di.Exists)
            {
                di.Create();
            }
            return di;
        }

        public void RemoveWorkingIndex() 
        {
            var working = GetWorkingFolder();
            working.GetFiles().ToList().ForEach(x => x.Delete());
            working.Delete(true);
        }

        public DirectoryInfo CreateFromTemplate()
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

        public void UpdateIndexPaths()
        {
            ExamineLuceneIndexes.Instance.Sets.Cast<IndexSet>().ToList()
                .ForEach(x =>
                {
                    x.IndexPath = GetWorkingFolder().FullName;
                });
        }

    }
}
