using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Examine;
using Examine.Providers;
using umbraco.cms.businesslogic;
using UmbracoExamine.DataServices;
using LuceneExamine;


namespace UmbracoExamine
{
    /// <summary>
    /// 
    /// </summary>
	public class UmbracoExamineIndexer : BaseLuceneExamineIndexer, IDisposable
    {
		/// <summary>
		/// A type that defines the type of index for each Umbraco field (non user defined fields)
		/// Alot of standard umbraco fields shouldn't be tokenized or even indexed, just stored into lucene
		/// for retreival after searching.
		/// </summary>
		private static readonly Dictionary<string, FieldIndexTypes> m_Definitions
			= new Dictionary<string, FieldIndexTypes>()
            {
                { "id", FieldIndexTypes.NOT_ANALYZED},
                { "version", FieldIndexTypes.NO},
                { "parentID", FieldIndexTypes.NO},
                { "level", FieldIndexTypes.NO},
                { "writerID", FieldIndexTypes.NO},
                { "creatorID", FieldIndexTypes.NO},
                { "nodeType", FieldIndexTypes.NOT_ANALYZED},
                { "template", FieldIndexTypes.NOT_ANALYZED},
                { "sortOrder", FieldIndexTypes.NO},
                { "createDate", FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { "updateDate", FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { "nodeName", FieldIndexTypes.ANALYZED},
                { "urlName", FieldIndexTypes.NOT_ANALYZED},
                { "writerName", FieldIndexTypes.NOT_ANALYZED},
                { "creatorName", FieldIndexTypes.NOT_ANALYZED},
                { "nodeTypeAlias", FieldIndexTypes.NOT_ANALYZED},
                { "path", FieldIndexTypes.NOT_ANALYZED}
            };

		public static IEnumerable<KeyValuePair<string, FieldIndexTypes>> GetPolicies()
		{
			return m_Definitions;
		}

		/// <summary>
		/// return the index policy for the field name passed in, if not found, return normal
		/// </summary>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public override FieldIndexTypes GetPolicy(string fieldName)
		{
			var def = m_Definitions.Where(x => x.Key == fieldName);
			return (def.Count() == 0 ? FieldIndexTypes.ANALYZED : def.Single().Value);
		}

		/// <summary>
		/// Deletes a node from the index.                
		/// </summary>
		/// <remarks>
		/// When a content node is deleted, we also need to delete it's children from the index so we need to perform a 
		/// custom Lucene search to find all decendents and create Delete item queues for them too.
		/// </remarks>
		/// <param name="nodeId">ID of the node to delete</param>
		public override void DeleteFromIndex(string nodeId)
		{
			//find all descendants based on path
			var descendantPath = string.Format(@"\-1\,*{0}\,*", nodeId);
			var rawQuery = string.Format("{0}:{1}", IndexPathFieldName, descendantPath);
			var c = m_InternalSearcher.CreateSearchCriteria();
			var filtered = c.RawQuery(rawQuery);
			var results = m_InternalSearcher.Search(filtered);

			//need to create a delete queue item for each one found
			foreach (var r in results)
			{
				SaveDeleteIndexQueueItem(new KeyValuePair<string, string>(IndexNodeIdFieldName, r.Id.ToString()));
			}

			base.DeleteFromIndex(nodeId);
		}

        /// <summary>
        /// Ensures that the node being indexed is of a correct type and is a descendent of the parent id specified.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
		protected virtual bool ValidateDocument(XElement node)
		{
			//check if this document is of a correct type of node type alias
			if (IndexerData.IncludeNodeTypes.Count() > 0)
				if (!IndexerData.IncludeNodeTypes.Contains(node.UmbNodeTypeAlias()))
					return false;

			//if this node type is part of our exclusion list, do not validate
			if (IndexerData.ExcludeNodeTypes.Count() > 0)
				if (IndexerData.ExcludeNodeTypes.Contains(node.UmbNodeTypeAlias()))
					return false;

			return base.ValidateDocument(node);
		}

		/// <summary>
		/// Collects all of the data that needs to be indexed as defined in the index set.
		/// </summary>
		/// <param name="node"></param>
		/// <returns>A dictionary representing the data which will be indexed</returns>
		protected override Dictionary<string, string> GetDataToIndex(XElement node, string type)
		{
			Dictionary<string, string> values = new Dictionary<string, string>();

			int nodeId = int.Parse(node.Attribute("id").Value);

			// Test for access if we're only indexing published content
			// return nothing if we're not supporting protected content and it is protected, and we're not supporting unpublished content
			if (!SupportUnpublishedContent && (!SupportProtectedContent && DataService.ContentService.IsProtected(nodeId, node.Attribute("path").Value)))
				return values;

			// Get all user data that we want to index and store into a dictionary 
			foreach (string fieldName in IndexerData.UserFields)
			{
				// Get the value of the data                
				string value = node.UmbSelectDataValue(fieldName);

				//raise the event and assign the value to the returned data from the event
				var indexingFieldDataArgs = new IndexingFieldDataEventArgs(node, fieldName, value, false, nodeId);
				OnGatheringFieldData(indexingFieldDataArgs);
				value = indexingFieldDataArgs.FieldValue;

				//don't add if the value is empty/null
				if (!string.IsNullOrEmpty(value))
				{
					if (!string.IsNullOrEmpty(value))
						values.Add(fieldName, DataService.ContentService.StripHtml(value));
				}
			}

			// Add umbraco node properties 
			foreach (string fieldName in IndexerData.StandardFields)
			{
				string val = node.UmbSelectPropertyValue(fieldName);
				var args = new IndexingFieldDataEventArgs(node, fieldName, val, true, nodeId);
				OnGatheringFieldData(args);
				val = args.FieldValue;

				//don't add if the value is empty/null                
				if (!string.IsNullOrEmpty(val))
				{
					values.Add(fieldName, val);
				}

			}

			//raise the event and assign the value to the returned data from the event
			var indexingNodeDataArgs = new IndexingNodeDataEventArgs(node, nodeId, values, type);
			OnGatheringNodeData(indexingNodeDataArgs);
			values = indexingNodeDataArgs.Fields;

			return values;
		}
    }
}
