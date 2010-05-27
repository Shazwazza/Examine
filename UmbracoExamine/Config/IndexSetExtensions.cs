using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine;
using UmbracoExamine.DataServices;

namespace UmbracoExamine.Config
{
    /// <summary>
    /// Extension methods for IndexSet
    /// </summary>
    public static class IndexSetExtensions
    {

        /// <summary>
        /// Convert the indexset to indexerdata
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        public static IIndexCriteria ToIndexCriteria(this IndexSet set)
        {
            return new IndexCriteria(
                set.IndexAttributeFields.ToList().Select(x => x.Name).ToArray(),
                set.IndexUserFields.ToList().Select(x => x.Name).ToArray(),
                set.IncludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                set.ExcludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                set.IndexParentId);
        }

        /// <summary>
        /// Returns a string array of all fields that are indexed including Umbraco fields
        /// </summary>
        public static IEnumerable<IndexField> CombinedUmbracoFields(this IndexSet set, IDataService svc)
        {
            if (set.IndexUserFields.Count == 0)
            {
                //we need to add all user fields to the collection if it is empty (this is the default if none are specified)
                //svc.ContentService.
            }

            return set.IndexUserFields.ToList()
                .Concat(set.IndexAttributeFields.ToList());
        }

      
    }
}
