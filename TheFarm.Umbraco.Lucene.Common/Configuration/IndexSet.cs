using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Web;
using UmbracoExamine;

namespace UmbracoExamine.Configuration
{
    public sealed class IndexSet : ConfigurationElement
    {

        [ConfigurationProperty("SetName", IsRequired = true, IsKey = true)]
        public string SetName
        {
            get
            {
                return (string)this["SetName"];
            }
            set
            {
                this["SetName"] = value;
            }
        }

        [ConfigurationProperty("IndexPath", IsRequired = true, IsKey = false)]
        public string IndexPath
        {
            get
            {
                return (string)this["IndexPath"];
            }
            set
            {
                this["IndexPath"] = value;
            }
        }

        /// <summary>
        /// Set this to configure the default maximum search results for an index set.
        /// If not set, 200 is the default.
        /// </summary>
        [ConfigurationProperty("MaxResults", IsRequired = false, IsKey = false)]
        public int MaxResults
        {
            get
            {
                if (this["MaxResults"] == null)
                    return 200;

                return (int)this["MaxResults"];
            }
            set
            {
                this["MaxResults"] = value;
            }
        }

        /// <summary>
        /// Returns the DirectoryInfo object for the index path.
        /// </summary>
        public DirectoryInfo IndexDirectory
        {
            get
            {
                return new DirectoryInfo(HttpContext.Current.Server.MapPath(this.IndexPath));
            }
        }

        /// <summary>
        /// When this property is set, the indexing will only index documents that are children of this node.
        /// </summary>
        [ConfigurationProperty("IndexParentId", IsRequired = false, IsKey = false)]
        public int? IndexParentId
        {
            get
            {
                if (this["IndexParentId"] == null)
                    return null;

                return (int)this["IndexParentId"];
            }
            set
            {
                this["IndexParentId"] = value;
            }
        }

        /// <summary>
        /// The collection of node types to index, if not specified, all node types will be indexed (apart from the ones specified in the ExcludeNodeTypes collection).
        /// </summary>
        [ConfigurationCollection(typeof(IndexFieldCollection))]
        [ConfigurationProperty("IncludeNodeTypes", IsDefaultCollection = false, IsRequired = false)]
        public IndexFieldCollection IncludeNodeTypes
        {
            get
            {
                return (IndexFieldCollection)base["IncludeNodeTypes"];
            }
        }

        /// <summary>
        /// The collection of node types to not index. If specified, these node types will not be indexed.
        /// </summary>
        [ConfigurationCollection(typeof(IndexFieldCollection))]
        [ConfigurationProperty("ExcludeNodeTypes", IsDefaultCollection = false, IsRequired = false)]
        public IndexFieldCollection ExcludeNodeTypes
        {
            get
            {
                return (IndexFieldCollection)base["ExcludeNodeTypes"];
            }
        }

        /// <summary>
        /// A collection of user defined umbraco fields to index
        /// </summary>
        [ConfigurationCollection(typeof(IndexFieldCollection))]
        [ConfigurationProperty("IndexUserFields", IsDefaultCollection = false, IsRequired = true)]
        public IndexFieldCollection IndexUserFields
        {
            get
            {
                return (IndexFieldCollection)base["IndexUserFields"];
            }
        }

        /// <summary>
        /// The fields umbraco values that will be indexed. i.e. id, nodeTypeAlias, writer, etc...
        /// </summary>
        [ConfigurationCollection(typeof(IndexFieldCollection))]
        [ConfigurationProperty("IndexUmbracoFields", IsDefaultCollection = false, IsRequired = true)]
        public IndexFieldCollection IndexUmbracoFields
        {
            get
            {
                return (IndexFieldCollection)base["IndexUmbracoFields"];
            }
        }


    }

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
