using System.Web;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace UmbracoExamine
{
    /// <summary>
    /// A handler that can be used to rebuild indexes which can be call asynchronously, or directly
    /// </summary>
    public class ExamineHandler : IHttpHandler
    {
        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            //make sure the user is authenticated
            if (umbraco.BasePages.UmbracoEnsuredPage.CurrentUser != null)
            {
                var index = string.Empty;
                var force = false;
                if (!string.IsNullOrEmpty(context.Request.QueryString["index"]))
                {
                    index = context.Request.QueryString["index"];
                }
                if (!string.IsNullOrEmpty(context.Request.QueryString["force"]))
                {
                    bool.TryParse(context.Request.QueryString["force"], out force);
                }
                if (!string.IsNullOrEmpty(index))
                {
                    DoRebuild(index, force);
                }
                else
                {
                    //if no index set is specified, then check all of them
                    foreach (BaseIndexProvider i in ExamineManager.Instance.IndexProviderCollection)
                    {
                        DoRebuild(i.Name, force);
                    }
                }
            }
        }

        private static void DoRebuild(string index, bool force)
        {
            var indexSet = ExamineManager.Instance.IndexProviderCollection[index];
            if (indexSet != null)
            {
                if (indexSet is LuceneIndexer)
                {
                    //check if the index exists, or if force is specified
                    var indexer = (LuceneIndexer)indexSet;
                    if (force || !IndexReader.IndexExists(new SimpleFSDirectory(indexer.LuceneIndexFolder)))
                    {
                        indexer.RebuildIndex();
                        return;
                    }
                }
            }
        }

        #endregion
    }
}
