using System;
using System.ComponentModel;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;

namespace Examine.Test.Search
{
    [Obsolete("Umbraco specific extension methods will be removed from Examine core in future version, query fields directly instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class LegacyExtensions
    {
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