using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Search
{
    public static class QueryExtensions
    {
        // TODO: In v2.0 we can look at moving these to directly to IQuery instead of a separate IFieldSelectableQuery with casting

        public static IOrdering SelectAllFields(this IBooleanOperation booleanOp)
        {
            var fieldQuery = ToFieldSelectableOrdering(booleanOp);
            return fieldQuery.SelectAllFields();
        }

        public static IOrdering SelectField(this IBooleanOperation booleanOp,string fieldName)
        {
            var fieldQuery = ToFieldSelectableOrdering(booleanOp);
            return fieldQuery.SelectField(fieldName);
        }

        public static IOrdering SelectFields(this IBooleanOperation booleanOp, params string[] fieldNames)
        {
            var fieldQuery = ToFieldSelectableOrdering(booleanOp);
            return fieldQuery.SelectFields(fieldNames);
        }

        public static IOrdering SelectFields(this IBooleanOperation booleanOp, ISet<string> fieldNames)
        {
            var fieldQuery = ToFieldSelectableOrdering(booleanOp);
            return fieldQuery.SelectFields(fieldNames);
        }

        public static IOrdering SelectFirstFieldOnly(this IBooleanOperation searchQuery)
        {
            var fieldQuery = ToFieldSelectableOrdering(searchQuery);
            return fieldQuery.SelectFirstFieldOnly();
        }

        private static IFieldSelectableOrdering ToFieldSelectableOrdering(IBooleanOperation booleanOp)
        {
            if(booleanOp is IFieldSelectableOrdering fieldSelectableQuery)
            {
                return fieldSelectableQuery;
            }
            throw new NotSupportedException("IFieldSelectableQuery is not supported");
        }

    }
}
