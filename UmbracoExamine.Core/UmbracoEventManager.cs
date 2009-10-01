using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.media;
using UmbracoExamine.Core.Config;
using umbraco;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using System.Web.Security;

namespace UmbracoExamine.Core
{
    public class UmbracoEventManager : ApplicationBase
    {

        public UmbracoEventManager() 
        {
            //don't bind event handlers if we're not suppose to listen
            if (!UmbracoExamineSettings.Instance.IndexProviders.EnableDefaultEventHandler)
                return;
            
            Media.AfterSave += new Media.SaveEventHandler(Media_AfterSave);
            Media.AfterDelete += new Media.DeleteEventHandler(Media_AfterDelete);
            
            //updates index after the cache is refreshed
            content.AfterUpdateDocumentCache += new content.DocumentCacheEventHandler(content_AfterUpdateDocumentCache);
            content.AfterClearDocumentCache += new content.DocumentCacheEventHandler(content_AfterClearDocumentCache);

            //TODO: This fires on publish too so need to change the update doc cache handlers to only 
            //index content for indexers that support published content only!
            Document.AfterSave += new Document.SaveEventHandler(Document_AfterSave);
            Document.AfterDelete += new Document.DeleteEventHandler(Document_AfterDelete);
        }

        
        /// <summary>
        /// Since this is on save, then get the data for the node from the raw data, not from cache
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// This will first check if there are any providers that require unpublished content
        /// </remarks>
        void Document_AfterSave(Document sender, SaveEventArgs e)
        {
            if (ExamineManager.Instance.IndexProviderCollection
                .Count(x => x.SupportUnpublishedContent) > 0)
            {
                ExamineManager.Instance.ReIndexNode(sender.ToXDocument(false).Root, IndexType.Content);
            }
        }

        /// <summary>
        /// Since this is on save, then get the data for the node from the raw data, not from cache
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// This will first check if there are any providers that require unpublished content
        /// </remarks>
        void Document_AfterDelete(Document sender, DeleteEventArgs e)
        {
            if (ExamineManager.Instance.IndexProviderCollection
                .Count(x => x.SupportUnpublishedContent) > 0)
            {
                ExamineManager.Instance.DeleteFromIndex(sender.ToXDocument(false).Root);
            }            
        }

        void Media_AfterDelete(Media sender, DeleteEventArgs e)
        {
            ExamineManager.Instance.DeleteFromIndex(sender.ToXDocument(false).Root);
        }

        void Media_AfterSave(Media sender, umbraco.cms.businesslogic.SaveEventArgs e)
        {
            ExamineManager.Instance.ReIndexNode(sender.ToXDocument(true).Root, IndexType.Media);
        }

        void content_AfterUpdateDocumentCache(Document sender, umbraco.cms.businesslogic.DocumentCacheEventArgs e)
        {
            ExamineManager.Instance.ReIndexNode(sender.ToXDocument(true).Root, IndexType.Content);
        }

        void content_AfterClearDocumentCache(Document sender, DocumentCacheEventArgs e)
        {
            ExamineManager.Instance.DeleteFromIndex(sender.ToXDocument(false).Root);
        }


        

    }
}
