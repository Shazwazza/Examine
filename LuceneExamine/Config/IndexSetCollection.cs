using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace LuceneExamine.Config
{
    public sealed class IndexSetCollection : ConfigurationElementCollection
    {
        #region Overridden methods to define collection
        protected override ConfigurationElement CreateNewElement()
        {
            return new IndexSet();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IndexSet)element).SetName;
        }
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
        protected override string ElementName
        {
            get
            {
                return "IndexSet";
            }
        }
        #endregion

        /// <summary>
        /// Default property for accessing Image Sets
        /// </summary>
        /// <param name="setType"></param>
        /// <returns></returns>
        public new IndexSet this[string setName]
        {
            get
            {
                return (IndexSet)this.BaseGet(setName);
            }
        }
    }
}
