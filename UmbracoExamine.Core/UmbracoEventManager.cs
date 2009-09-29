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

            //TODO: These should only fire if indexing is happening for non-published items!
            Document.AfterSave += new Document.SaveEventHandler(Document_AfterSave);
            Document.AfterDelete += new Document.DeleteEventHandler(Document_AfterDelete);
        }

        void Document_AfterSave(Document sender, SaveEventArgs e)
        {
            ExamineManager.Instance.ReIndexNode(sender, IndexType.Content);
        }

        void Document_AfterDelete(Document sender, DeleteEventArgs e)
        {            
            ExamineManager.Instance.DeleteFromIndex(sender);
        }

        void Media_AfterDelete(Media sender, DeleteEventArgs e)
        {
            ExamineManager.Instance.DeleteFromIndex(sender);
        }

        void content_AfterUpdateDocumentCache(Document sender, umbraco.cms.businesslogic.DocumentCacheEventArgs e)
        {
            ExamineManager.Instance.ReIndexNode(sender, IndexType.Content);
        }

        void Media_AfterSave(Media sender, umbraco.cms.businesslogic.SaveEventArgs e)
        {
            ExamineManager.Instance.ReIndexNode(sender, IndexType.Media);
        }

    }
}
