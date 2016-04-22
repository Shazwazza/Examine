using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Examine.Providers;
using Examine.SearchCriteria;
using Lucene.Net.Index;

namespace Examine
{
    [Obsolete("Umbraco specific extension methods will be removed from Examine core in future version, query fields directly instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class LegacyExtensions
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This is only exposed for backward compatibility reasons, it should not be used directly")]
        public static IndexWriter GetIndexWriter(this LuceneIndexer luceneIndexer)
        {
            return luceneIndexer.SearcherContext.Writer;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Do not use this, use the other DeleteFromIndex method instead")]
        public static void DeleteFromIndex(this ExamineManager manager, string nodeId, IEnumerable<BaseIndexProvider> providers)
        {
            _DeleteFromIndex(nodeId, providers);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Do not use this, use the other DeleteFromIndex method instead")]
        public static void DeleteFromIndex(this ExamineManager manager, string nodeId)
        {
            _DeleteFromIndex(nodeId, manager.IndexProviderCollection);
        }
        private static void _DeleteFromIndex(string nodeId, IEnumerable<BaseIndexProvider> providers)
        {
            foreach (var provider in providers)
            {
                provider.DeleteFromIndex(nodeId);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Do not use this, use the IndexItems method instead")]
        public static void ReIndexNode(this ExamineManager manager, XElement node, string category, IEnumerable<BaseIndexProvider> providers)
        {
            _ReIndexNode(node, category, providers);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Do not use this, use the IndexItems method instead")]
        public static void ReIndexNode(this ExamineManager manager, XElement node, string category)
        {
            _ReIndexNode(node, category, manager.IndexProviderCollection);
        }
        private static void _ReIndexNode(XElement node, string type, IEnumerable<BaseIndexProvider> providers)
        {
            foreach (var provider in providers)
            {
                provider.ReIndexNode(node, type);
            }
        }

        [Obsolete("Umbraco specific extension methods will be removed from Examine core in future version, query fields directly instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IBooleanOperation ParentId(this IQuery query, int id)
        {
            var fieldQuery = query.Field("parentID", id);
            return fieldQuery;
        }
        [Obsolete("Umbraco specific extension methods will be removed from Examine core in future version, query fields directly instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IBooleanOperation NodeName(this IQuery query, string nodeName)
        {
            var fieldQuery = query.Field("nodeName", (IExamineValue)new ExamineValue(Examineness.Explicit, nodeName));
            return fieldQuery;
        }
        [Obsolete("Umbraco specific extension methods will be removed from Examine core in future version, query fields directly instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IBooleanOperation NodeName(this IQuery query, IExamineValue nodeName)
        {
            var fieldQuery = query.Field("nodeName", nodeName);
            return fieldQuery;
        }
        [Obsolete("Umbraco specific extension methods will be removed from Examine core in future version, query fields directly instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IBooleanOperation NodeTypeAlias(this IQuery query, string nodeTypeAlias)
        {
            var fieldQuery = query.Field("__NodeTypeAlias", (IExamineValue)new ExamineValue(Examineness.Explicit, nodeTypeAlias));
            return fieldQuery;
        }
        [Obsolete("Umbraco specific extension methods will be removed from Examine core in future version, query fields directly instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IBooleanOperation NodeTypeAlias(this IQuery query, IExamineValue nodeTypeAlias)
        {
            var fieldQuery = query.Field("__NodeTypeAlias", nodeTypeAlias);
            return fieldQuery;
        }
    }
}