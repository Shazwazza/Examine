using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace UmbracoExamine.Config
{
    public sealed class IndexFieldCollection : ConfigurationElementCollection
    {
        #region Overridden methods to define collection
        protected override ConfigurationElement CreateNewElement()
        {
            return new IndexField();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            IndexField field = (IndexField)element;
            return field.Name;
        }
        #endregion

        public void Add(IndexField field)
        {
            BaseAdd(field, true);
        }

        /// <summary>
        /// Default property for accessing an IndexField definition
        /// </summary>
        /// <value>Field Name</value>
        /// <returns></returns>
        public new IndexField this[string name]
        {
            get
            {
                return (IndexField)this.BaseGet(name);
            }
        }

    }
}
