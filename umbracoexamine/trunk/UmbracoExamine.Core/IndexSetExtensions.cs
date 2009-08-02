using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoExamine.Configuration;

namespace UmbracoExamine.Core
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
        public static IndexerData ToIndexerData(this IndexSet set)
        {
            return new IndexerData(
                set.IndexUmbracoFields.ToList().Select(x => x.Name).ToArray(),
                set.IndexUserFields.ToList().Select(x => x.Name).ToArray(),
                set.IndexDirectory.FullName,
                set.IncludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                set.ExcludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                set.IndexParentId,
                set.MaxResults);
        }
    }
}
