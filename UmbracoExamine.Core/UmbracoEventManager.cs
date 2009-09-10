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
            Media.AfterSave += new Media.SaveEventHandler(Media_AfterSave);
            content.AfterUpdateDocumentCache += new content.DocumentCacheEventHandler(content_AfterUpdateDocumentCache);         
        }

        void content_AfterUpdateDocumentCache(Document sender, umbraco.cms.businesslogic.DocumentCacheEventArgs e)
        {
            if (IndexProvidersSection.Instance.EnabledDefaultEventHandler)
                ExamineManager.Instance.ReIndexNode(sender.Id, IndexType.Content);
        }

        void Media_AfterSave(Media sender, umbraco.cms.businesslogic.SaveEventArgs e)
        {
            if (IndexProvidersSection.Instance.EnabledDefaultEventHandler)
                ExamineManager.Instance.ReIndexNode(sender.Id, IndexType.Media);
        }

    }
}
