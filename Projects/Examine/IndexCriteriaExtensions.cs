using System;
using System.Collections.Generic;
using System.Linq;

namespace Examine
{
    [Obsolete("Should not use IIndexCriteria anymore")]
    internal static class IndexCriteriaExtensions
    {
        public static IEnumerable<IIndexField> AllFields(this IIndexCriteria criteria)
        {
            return criteria.StandardFields.Concat(criteria.UserFields);
        }
    }
}