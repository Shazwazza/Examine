using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Web;

namespace UmbracoExamine.Providers.Config
{
    /// <summary>
    /// Defines XPath statements that map to specific umbraco nodes
    /// </summary>
    public sealed class ExamineLuceneIndexes : ConfigurationSection
    {

        #region Singleton definition

        private static readonly ExamineLuceneIndexes m_IndexSets;
        private ExamineLuceneIndexes() { }
        static ExamineLuceneIndexes()
        {
            m_IndexSets = ConfigurationManager.GetSection(SectionName) as ExamineLuceneIndexes;     
  
        }
        public static ExamineLuceneIndexes Instance
        {
            get { return m_IndexSets; }
        }

        #endregion

        private const string SectionName = "ExamineLuceneIndexSets";

        [ConfigurationCollection(typeof(IndexSetCollection))]
        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
        public IndexSetCollection Sets
        {
            get
            {
                return (IndexSetCollection)base[""];
            }
        }

        /// <summary>
        /// Specifies the default index set to use for the default action handler.
        /// </summary>
        [ConfigurationProperty("DefaultIndexSet", IsRequired = true)]
        public string DefaultIndexSet
        {
            get
            {
                return (string)this["DefaultIndexSet"];
            }
        }

        /// <summary>
        /// If true, the IndexingActionHandler will be run to keep the default index up to date.
        /// </summary>
        [ConfigurationProperty("EnableDefaultActionHandler", IsRequired = true)]
        public bool EnableDefaultActionHandler
        {
            get
            {
                return (bool)this["EnableDefaultActionHandler"];
            }
        }
                
    }

    
    
}