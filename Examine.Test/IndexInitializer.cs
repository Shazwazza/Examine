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
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis;
using Examine.Providers;
using System.Collections.Specialized;

namespace Examine.Test
{
    /// <summary>
    /// Used internally by test classes to initialize a new index from the template
    /// </summary>
    internal class IndexInitializer
    {
        public IndexInitializer()
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
            var searchTest = GetWorkingFolder().GetDirectories("App_Data").First().GetDirectories("SearchWorkingTest").FirstOrDefault();
            RemoveIndex(searchTest, "CWSIndex");

            var indexText = GetWorkingFolder().GetDirectories("App_Data").First().GetDirectories("IndexWorkingTest").FirstOrDefault();
            RemoveIndex(indexText, "CWSSearch");
        }

        private void RemoveIndex(DirectoryInfo di, string indexName)
        {
            if (di != null)
            {
                var cwsSearch = (LuceneExamineSearcher)ExamineManager.Instance.SearchProviderCollection[indexName];

                try
                {
                    var s = cwsSearch.GetSearcher();
                    var r = s.GetIndexReader();
                    s.Close();
                    r.Close();
                }
                catch (ApplicationException)
                {
                    //do nothing, this is because there is no index to search on
                }

                SimpleFSDirectory searchFolder = new SimpleFSDirectory(di);
                if (IndexWriter.IsLocked(searchFolder))
                {
                    IndexWriter.Unlock(searchFolder);
                }

                var writer = new IndexWriter(new SimpleFSDirectory(di), new SimpleAnalyzer(), true, IndexWriter.MaxFieldLength.UNLIMITED);
                writer.Close();

                di.GetFiles().ToList().ForEach(x => x.Delete());
                di.Delete(true);
            }         
        }

        private void UpdateIndexPaths()
        {
            var template = GetTemplateFolder();
            ExamineLuceneIndexes.Instance.Sets.Cast<IndexSet>().ToList()
                .ForEach(x =>
                {
                    //updtae the index set paths
                    x.IndexPath = Path.Combine(GetWorkingFolder().FullName, x.IndexPath);
                    
                    var di = new DirectoryInfo(x.IndexPath);
                    if (!di.Exists)
                    {
                        di.Create();
                    }

                    var indexDir = di.CreateSubdirectory("Index");
                    template.GetFiles().ToList().ForEach(f => f.CopyTo(Path.Combine(indexDir.FullName, f.Name)));

                    //set the searcher that goes with this index set
                    var searcher = ExamineManager.Instance.SearchProviderCollection.Cast<LuceneExamineSearcher>()
                        .Where(s => s.IndexSetName == x.SetName)
                        .Single();

                    searcher.LuceneIndexFolder = indexDir;

                });

            //ExamineManager.Instance.SearchProviderCollection.Cast<BaseSearchProvider>().ToList()
            //    .ForEach(x =>
            //    {
            //        var p = new NameValueCollection();
            //        p.Add("indexSet", "");
            //        x.Initialize(x.Name, p);
            //        //x.LuceneIndexFolder = new DirectoryInfo(Path.Combine(ExamineLuceneIndexes.Instance.Sets[x.].IndexDirectory.FullName, "Index"));
            //    });
        }

    }
}
