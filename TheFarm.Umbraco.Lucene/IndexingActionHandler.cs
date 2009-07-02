using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.BusinessLogic.Actions;
using umbraco.interfaces;
using umbraco.cms.businesslogic.web;
using UmbracoExamine.Configuration;
using UmbracoExamine;

namespace UmbracoExamine.Lucene1
{

    /// <summary>
    /// Handles certain actions of Umbraco that require re-indexing/removing nodes from the Lucene index.
    /// If you need to use a custom indexer that is inheriting from the UmbracoIndexer, you can turn off EnableDefaultActionHandler in the config 
    /// and inherit from this class. You'll need to override the Enabled property and set it to true. Then you can override the Indexer property
    /// to use your own custom one.
    /// </summary>
    public class IndexingActionHandler : IActionHandler
    {
        
        public IndexingActionHandler()
        {
            m_Actions = CreateActionList();

            m_Indexer = new UmbracoIndexer(IndexSets.Instance.DefaultIndexSet);
        }       

        private List<IAction> m_Actions;
        private IUmbracoIndexer m_Indexer;

        /// <summary>
        /// Override this method to use a custom indexer
        /// </summary>
        protected virtual IUmbracoIndexer Indexer
        {
            get
            {
                return m_Indexer;
            }
        }

        /// <summary>
        /// If inheriting from this class, this needs to be overridden and set to true if the EnableDefaultActionHandler is set to false in the config.
        /// </summary>
        protected virtual bool IsEnabled
        {
            get
            {
                return IndexSets.Instance.EnableDefaultActionHandler;
            }
        }

        protected virtual List<IAction> CreateActionList()
        {
            return new List<IAction>()
            {
                new ActionPublish(),
                new ActionUnPublish(),
                new ActionMove(),
                new ActionDelete()
            };
        }

        #region IActionHandler Members

        /// <summary>
        /// Checks if we should reindex or remove nodes.
        /// The script only runs if the EnableDefaultActionHandler is enabled in the config.
        /// </summary>
        /// <param name="documentObject"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public virtual bool Execute(Document documentObject, IAction action)
        {
            if (!IsEnabled)
                return true;

            int nodeId = documentObject.Id;

            if (ReIndexRequired(action))
                Indexer.ReIndexNode(nodeId);
            else if (DeleteIndexRequired(action))
                Indexer.DeleteFromIndex(nodeId);
           

            return true;
        }

        protected virtual bool DeleteIndexRequired(IAction action)
        {
            List<IAction> actions = new List<IAction>() 
            {
                new ActionUnPublish(),
                new ActionDelete(),
            };
            if (actions.Select(x => x.Alias).ToArray().Contains(action.Alias))
                return true;

            return false;   
                    
        }

        protected virtual bool ReIndexRequired(IAction action)
        {
            List<IAction> actions = new List<IAction>() 
            {
                new ActionPublish(),
                new ActionMove(),
            };
            if (actions.Select(x => x.Alias).ToArray().Contains(action.Alias))
                return true;

            return false;    
        }

        public string HandlerName()
        {
            return "UmbracoExamine.IndexingActionHandler";
        }

        public IAction[] ReturnActions()
        {
            return m_Actions.ToArray();
        }

        #endregion
    }
}
