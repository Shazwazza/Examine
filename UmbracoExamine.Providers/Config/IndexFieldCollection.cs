using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace UmbracoExamine.Providers.Config
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

        /// <summary>
        /// Default property for accessing an IndexField definition
        /// </summary>
        /// <param name="imageSize"></param>
        /// <returns></returns>
        public IndexField this[string name]
        {
            get
            {
                return (IndexField)this.BaseGet(name);
            }
        }

    }


    public static class IndexFieldCollectionExtensions
    {
        public static List<IndexField> ToList(this IndexFieldCollection indexes)
        {
            List<IndexField> fields = new List<IndexField>();
            foreach (IndexField field in indexes)
                fields.Add(field);
            return fields;
        }
    }
}
