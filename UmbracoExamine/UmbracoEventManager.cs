using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using Examine.Config;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.web;
using Examine;
using Examine.Providers;
using umbraco.cms.businesslogic.member;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Store;

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
            var registeredProviders = ExamineManager.Instance.IndexProviderCollection
                .Where(x => x.EnableDefaultEventHandler)
                .Count();

            Log.Add(LogTypes.Custom, -1, "[UmbracoExamine] Adding examine event handlers for index providers: " + registeredProviders.ToString());

            //don't bind event handlers if we're not suppose to listen
            if (registeredProviders == 0)
                return;

            EnsureIndexesExist();

            Media.AfterSave += Media_AfterSave;
            Media.AfterDelete += Media_AfterDelete;
            CMSNode.AfterMove += Media_AfterMove;

            //These should only fire for providers that DONT have SupportUnpublishedContent set to true
            content.AfterUpdateDocumentCache += content_AfterUpdateDocumentCache;
            content.AfterClearDocumentCache += content_AfterClearDocumentCache;

            //These should only fire for providers that have SupportUnpublishedContent set to true
            Document.AfterSave += Document_AfterSave;
            Document.AfterDelete += Document_AfterDelete;

            Member.AfterSave += Member_AfterSave;
            Member.AfterDelete += Member_AfterDelete;
        }

        /// <summary>
        /// This makes sure the Lucene indexes themselves exist and rebuilds them on app startup if none are there.
        /// </summary>
        private void EnsureIndexesExist()
        {
            if (!ExamineSettings.Instance.RebuildOnAppStart)
                return;

            foreach(var indexSet in ExamineManager.Instance.IndexProviderCollection)
            {
                if (indexSet is LuceneIndexer)
                {
                    var indexer = (LuceneIndexer)indexSet;
                    if (!IndexReader.IndexExists(new SimpleFSDirectory(indexer.LuceneIndexFolder)))
                    {
                        indexer.RebuildIndex();
                        return;
                    }
                }
            }
        }


        //This does work, however we need to lock down the httphandler and thats an issue... so i've removed thsi
        //if people don't want to rebuild on app startup then they can disable it and reindex manually.

        /////<summary>
        ///// Calls a web request in a worker thread to rebuild the indexes
        /////</summary>
        //public void RebuildIndexAsync()
        //{
        //    if (HttpContext.Current != null)
        //    {
        //        var handler = VirtualPathUtility.ToAbsolute(ExamineSettings.Instance.WebHandlerPath);
        //        var fullPath = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + handler;
        //        var thread = new Thread(() =>
        //                                 {
        //                                     var request = (HttpWebRequest)WebRequest.Create(fullPath);
        //                                     request.Timeout = Timeout.Infinite;
        //                                     request.UseDefaultCredentials = true;
        //                                     request.Method = "GET";
        //                                     request.Proxy = null;

        //                                     HttpWebResponse response;
        //                                     try
        //                                     {
        //                                          response = (HttpWebResponse)request.GetResponse();

        //                                          if (response.StatusCode != HttpStatusCode.OK)
        //                                          {
        //                                              Log.Add(LogTypes.Custom, -1, "[UmbracoExamine] ExamineHandler request ended with an error: " + response.StatusDescription);
        //                                          }
        //                                     }
        //                                     catch (WebException ex)
        //                                     {
        //                                         Log.Add(LogTypes.Custom, -1, "[UmbracoExamine] ExamineHandler request threw an exception: " + ex.Message);
        //                                     }

        //                                 }) { IsBackground = true, Name = "ExamineAsyncHandler" };

        //        thread.Start();

                
        //    }

        //}


        void Member_AfterSave(Member sender, SaveEventArgs e)
        {
            //ensure that only the providers are flagged to listen execute
            var xml = sender.ToXml(new System.Xml.XmlDocument(), false).ToXElement();
            var providers = ExamineManager.Instance.IndexProviderCollection.Where(x => x.EnableDefaultEventHandler);
            ExamineManager.Instance.ReIndexNode(xml, IndexTypes.Member, providers);
        }

        void Member_AfterDelete(Member sender, DeleteEventArgs e)
        {
            var nodeId = sender.Id.ToString();

            //ensure that only the providers are flagged to listen execute
            ExamineManager.Instance.DeleteFromIndex(nodeId,
                ExamineManager.Instance.IndexProviderCollection
                    .Where(x => x.EnableDefaultEventHandler));
        }



        /// <summary>
        /// Only index using providers that SupportUnpublishedContent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Document_AfterSave(Document sender, SaveEventArgs e)
        {
            //ensure that only the providers that have unpublishing support enabled     
            //that are also flagged to listen

            //there's a bug in 4.0.x that fires the Document saving event handler for media when media is moved,
            //therefore, we need to try to figure out if this is media or content. Currently one way to do this
            //is by checking the creator ID property as it will be null if it is media. We then need to try to 
            //create the media object, see if it exists, and pass it to the media save event handler... yeah i know, 
            //pretty horrible but has to be done.

            try
            {
                var creator = sender.Creator;
                if (creator != null)
                {
                    //it's def a Document
                    ExamineManager.Instance.ReIndexNode(sender.ToXDocument(false).Root, IndexTypes.Content,
                        ExamineManager.Instance.IndexProviderCollection
                            .Where(x => x.SupportUnpublishedContent
                                && x.EnableDefaultEventHandler));

                    return; //exit, we've indexed the content
                }
            }
            catch (Exception)
            {
                //if we get this exception, it means it's most likely media, so well do our check next.   
            }

            //this is most likely media, not sure what kind of exception might get thrown in 4.0.x or 4.1 if accessing a null
            //creator, so we catch everything.

            var m = new Media(sender.Id);
            if (!string.IsNullOrEmpty(m.Path))
            {
                //this is a media item, no exception occurred on access to path and it's not empty which means it was found
                Media_AfterSave(m, e);
                return;
            }


        }

        /// <summary>
        /// Only remove indexes using providers that SupportUnpublishedContent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Document_AfterDelete(Document sender, DeleteEventArgs e)
        {
            var nodeId = sender.Id.ToString();

            //ensure that only the providers that have unpublishing support enabled      
            //that are also flagged to listen
            ExamineManager.Instance.DeleteFromIndex(nodeId,
                ExamineManager.Instance.IndexProviderCollection
                    .Where(x => x.SupportUnpublishedContent
                        && x.EnableDefaultEventHandler));
        }

        void Media_AfterDelete(Media sender, DeleteEventArgs e)
        {
            var nodeId = sender.Id.ToString();

            //ensure that only the providers are flagged to listen execute
            ExamineManager.Instance.DeleteFromIndex(nodeId,
                ExamineManager.Instance.IndexProviderCollection
                    .Where(x => x.EnableDefaultEventHandler));
        }

        void Media_AfterSave(Media sender, umbraco.cms.businesslogic.SaveEventArgs e)
        {
            //ensure that only the providers are flagged to listen execute
            IndexMedia(sender);
        }

        private void IndexMedia(Media sender)
        {
            ExamineManager.Instance.ReIndexNode(sender.ToXDocument(true).Root, IndexTypes.Media,
                                                ExamineManager.Instance.IndexProviderCollection
                                                    .Where(x => x.EnableDefaultEventHandler));
        }

        /// <summary>
        /// When media is moved, re-index
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Media_AfterMove(object sender, MoveEventArgs e)
        {
            if (sender is Media)
            {
                Media m = (Media)sender;
                IndexMedia(m);
            }
        }

        /// <summary>
        /// Only Update indexes for providers that dont SupportUnpublishedContent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void content_AfterUpdateDocumentCache(Document sender, umbraco.cms.businesslogic.DocumentCacheEventArgs e)
        {
            //ensure that only the providers that have DONT unpublishing support enabled       
            //that are also flagged to listen
            ExamineManager.Instance.ReIndexNode(sender.ToXDocument(true).Root, IndexTypes.Content,
                ExamineManager.Instance.IndexProviderCollection
                    .Where(x => !x.SupportUnpublishedContent
                        && x.EnableDefaultEventHandler));
        }

        /// <summary>
        /// Only update indexes for providers that don't SupportUnpublishedContnet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void content_AfterClearDocumentCache(Document sender, DocumentCacheEventArgs e)
        {
            var nodeId = sender.Id.ToString();
            //ensure that only the providers that DONT have unpublishing support enabled           
            //that are also flagged to listen
            ExamineManager.Instance.DeleteFromIndex(nodeId,
                ExamineManager.Instance.IndexProviderCollection
                    .Where(x => !x.SupportUnpublishedContent
                        && x.EnableDefaultEventHandler));
        }

    }
}
