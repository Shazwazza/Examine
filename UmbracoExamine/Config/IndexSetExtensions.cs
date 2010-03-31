using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine;

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
        public static IEnumerable<string> CombinedUmbracoFields(this IndexSet set)
        {
            return set.IndexUserFields.ToList().Select(x => x.Name)
                .Concat(set.IndexAttributeFields.ToList().Select(x => x.Name));
        }

      
    }
}
