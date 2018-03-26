using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine;
using UmbracoExamine.DataServices;
using Examine.LuceneEngine.Config;

namespace UmbracoExamine.Config
{
    /// <summary>
    /// Extension methods for IndexSet
    /// </summary>
    internal static class IndexSetExtensions
    {

        private static readonly object Locker = new object();

        /// <summary>
        /// Convert the indexset to indexerdata.
        /// This detects if there are no user/system fields specified and if not, uses the data service to look them 
        /// up and update the in memory IndexSet.
        /// </summary>
        /// <param name="set"></param>
        /// <param name="svc"></param>
        /// <returns></returns>
        public static ConfigIndexCriteria ToIndexCriteria(this IndexSet set, IDataService svc)
        {
            if (set.IndexUserFields.Count == 0)
            {
                lock (Locker)
                {
                    //we need to add all user fields to the collection if it is empty (this is the default if none are specified)
                    var userFields = svc.ContentService.GetAllUserPropertyNames();
                    foreach (var u in userFields)
                    {
                        set.IndexUserFields.Add(new ConfigIndexField() { Name = u });
                    } 
                }
            }

            if (set.IndexAttributeFields.Count == 0)
            {
                lock (Locker)
                {
                    //we need to add all system fields to the collection if it is empty (this is the default if none are specified)
                    var sysFields = svc.ContentService.GetAllSystemPropertyNames();
                    foreach (var s in sysFields)
                    {
                        set.IndexAttributeFields.Add(new ConfigIndexField() { Name = s });
                    } 
                }
            }

            return new ConfigIndexCriteria(
                set.IndexAttributeFields.Cast<ConfigIndexField>().Select(x => new FieldDefinition(x.Name, x.Type)).ToArray(),
                set.IndexUserFields.Cast<ConfigIndexField>().Select(x => new FieldDefinition(x.Name, x.Type)).ToArray(),
                set.IncludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                set.ExcludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                set.IndexParentId);
        }      
      
    }
}
