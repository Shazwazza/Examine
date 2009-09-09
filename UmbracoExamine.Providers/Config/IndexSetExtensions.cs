using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoExamine.Core;

namespace UmbracoExamine.Providers.Config
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
        public static IIndexCriteria ToIndexerData(this IndexSet set)
        {
            return new IndexCriteria(
                set.IndexUmbracoFields.ToList().Select(x => x.Name).ToArray(),
                set.IndexUserFields.ToList().Select(x => x.Name).ToArray(),
                set.IncludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                set.ExcludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                set.IndexParentId);
        }
    }
}
