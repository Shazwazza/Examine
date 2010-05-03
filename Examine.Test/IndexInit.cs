using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UmbracoExamine.Config;
using UmbracoExamine;
using Examine.Test.DataServices;
using System.Threading;

namespace Examine.Test
{
    public class IndexInit
    {
        public IndexInit()
        {
            RemoveWorkingIndex();
            UpdateIndexPaths();            
        }

        private DirectoryInfo GetTemplateFolder() {
            var template = GetWorkingFolder()
                .GetDirectories("App_Data")
                .Single()
                .GetDirectories("TemplateIndex")
                .Single();
            return template;
        }

        private DirectoryInfo GetWorkingFolder()
        {
            return new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
        }

        public void RemoveWorkingIndex() 
        {
            //need to wait between test classes so lucene closes it's locks!
            Thread.Sleep(2000);

            var searchTest = GetWorkingFolder().GetDirectories("App_Data").First().GetDirectories("SearchWorkingTest").FirstOrDefault();
            if (searchTest != null)
            {
                searchTest.GetFiles().ToList().ForEach(x => x.Delete());
                searchTest.Delete(true);
            }            

            var indexText = GetWorkingFolder().GetDirectories("App_Data").First().GetDirectories("IndexWorkingTest").FirstOrDefault();
            if (indexText != null)
            {
                indexText.GetFiles().ToList().ForEach(x => x.Delete());
                indexText.Delete(true);
            }
            
        }


        private void UpdateIndexPaths()
        {
            var template = GetTemplateFolder();
            ExamineLuceneIndexes.Instance.Sets.Cast<IndexSet>().ToList()
                .ForEach(x =>
                {
                    x.IndexPath = Path.Combine(GetWorkingFolder().FullName, x.IndexPath);
                    var di = new DirectoryInfo(x.IndexPath);
                    if (!di.Exists)
                    {
                        di.Create();
                    }

                    var indexDir = di.CreateSubdirectory("Index");
                    template.GetFiles().ToList().ForEach(f => f.CopyTo(Path.Combine(indexDir.FullName, f.Name)));
                });
        }

    }
}
