using System.Linq;
using Examine.Config;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.web;
using Examine;

namespace UmbracoExamine
{
    /// <summary>
    /// An <see cref="umbraco.BusinessLogic.ApplicationBase"/> instance for wiring up Examine to the Umbraco events system
    /// </summary>
    public class UmbracoEventManager : ApplicationBase
    {
        /// <summary>
        /// Creates a new instance of the class
        /// </summary>
        public UmbracoEventManager() 
        {
            //don't bind event handlers if we're not suppose to listen
            if (!ExamineSettings.Instance.IndexProviders.EnableDefaultEventHandler)
                return;

            Log.Add(LogTypes.Custom, -1, "[UmbracoExamine] Adding examine event handlers ");            

            Media.AfterSave += new Media.SaveEventHandler(Media_AfterSave);
            Media.AfterDelete += new Media.DeleteEventHandler(Media_AfterDelete);

            //These should only fire for providers that DONT have SupportUnpublishedContent set to true
            content.AfterUpdateDocumentCache += new content.DocumentCacheEventHandler(content_AfterUpdateDocumentCache);
            content.AfterClearDocumentCache += new content.DocumentCacheEventHandler(content_AfterClearDocumentCache);

            //These should only fire for providers that have SupportUnpublishedContent set to true
            Document.AfterSave += new Document.SaveEventHandler(Document_AfterSave);
            Document.AfterDelete += new Document.DeleteEventHandler(Document_AfterDelete);
        }

        /// <summary>
        /// Only index using providers that SupportUnpublishedContent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Document_AfterSave(Document sender, SaveEventArgs e)
        {
            //ensure that only the providers that have unpublishing support enabled           
            ExamineManager.Instance.ReIndexNode(sender.ToXDocument(false).Root, IndexType.Content, 
                ExamineManager.Instance.IndexProviderCollection
                    .Where(x => x.SupportUnpublishedContent));            
        }

        /// <summary>
        /// Only remove indexes using providers that SupportUnpublishedContent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Document_AfterDelete(Document sender, DeleteEventArgs e)
        {
            //ensure that only the providers that have unpublishing support enabled           
            ExamineManager.Instance.DeleteFromIndex(sender.ToXDocument(false).Root,
                ExamineManager.Instance.IndexProviderCollection
                    .Where(x => x.SupportUnpublishedContent));               
        }

        void Media_AfterDelete(Media sender, DeleteEventArgs e)
        {
            ExamineManager.Instance.DeleteFromIndex(sender.ToXDocument(false).Root);
        }

        void Media_AfterSave(Media sender, umbraco.cms.businesslogic.SaveEventArgs e)
        {
            ExamineManager.Instance.ReIndexNode(sender.ToXDocument(true).Root, IndexType.Media);
        }

        /// <summary>
        /// Only Update indexes for providers that dont SupportUnpublishedContent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void content_AfterUpdateDocumentCache(Document sender, umbraco.cms.businesslogic.DocumentCacheEventArgs e)
        {
            //ensure that only the providers that have DONT unpublishing support enabled           
            ExamineManager.Instance.ReIndexNode(sender.ToXDocument(false).Root, IndexType.Content,
                ExamineManager.Instance.IndexProviderCollection
                    .Where(x => !x.SupportUnpublishedContent));            
        }

        /// <summary>
        /// Only update indexes for providers that don't SupportUnpublishedContnet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void content_AfterClearDocumentCache(Document sender, DocumentCacheEventArgs e)
        {
            //ensure that only the providers that DONT have unpublishing support enabled           
            ExamineManager.Instance.DeleteFromIndex(sender.ToXDocument(false).Root,
                ExamineManager.Instance.IndexProviderCollection
                    .Where(x => !x.SupportUnpublishedContent));   
        }


        

    }
}
