using System;
using System.Collections;
using System.Collections.Generic;

namespace Examine.SearchCriteria
{
    public static class QueryExtensions
    {
        public static IBooleanOperation NativeQuery(this IQuery searchQuery, string query, ISet<string> loadedFieldNames = null)
        {
            var fieldQuery = ToFieldSelectableQuery(searchQuery);
            return fieldQuery.NativeQuery(query, loadedFieldNames);
        }

        public static IBooleanOperation SelectAllFields(this IQuery searchQuery)
        {
            var fieldQuery = ToFieldSelectableQuery(searchQuery);
            return fieldQuery.SelectAllFields();
        }

        public static IBooleanOperation SelectField(this IQuery searchQuery, string fieldName)
        {
            var fieldQuery = ToFieldSelectableQuery(searchQuery);
            return fieldQuery.SelectField(fieldName);
        }

        public static IBooleanOperation SelectFields(this IQuery searchQuery, params string[] fieldNames)
        {
            var fieldQuery = ToFieldSelectableQuery(searchQuery);
            return fieldQuery.SelectFields(fieldNames);
        }

        public static IBooleanOperation SelectFields(this IQuery searchQuery, Hashtable fieldNames)
        {
            var fieldQuery = ToFieldSelectableQuery(searchQuery);
            return fieldQuery.SelectFields(fieldNames);
        }

        public static IBooleanOperation SelectFirstFieldOnly(this IQuery searchQuery)
        {
            var fieldQuery = ToFieldSelectableQuery(searchQuery);
            return fieldQuery.SelectFirstFieldOnly();
        }

        private static IFieldSelectableQuery ToFieldSelectableQuery(IQuery searchQuery)
        {
            if (searchQuery is IFieldSelectableQuery fieldSelectableQuery)
            {
                return fieldSelectableQuery;
            }
            throw new NotSupportedException("IFieldSelectableQuery is not supported");
        }

    }
}