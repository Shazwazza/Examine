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

    }
}