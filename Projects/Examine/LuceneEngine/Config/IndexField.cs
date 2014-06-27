using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Examine.LuceneEngine.Config
{
    ///<summary>
    /// A configuration item representing a field to index
    ///</summary>
    public sealed class IndexField : ConfigurationElement, IIndexField
    {
        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["Name"];
            }
            set
            {
                this["Name"] = value;
            }
        }

        [ConfigurationProperty("EnableSorting", IsRequired = false)]
        public bool EnableSorting
        {
            get
            {
                return (bool)this["EnableSorting"];
            }
            set
            {
                this["EnableSorting"] = value;
            }
        }

        [ConfigurationProperty("Type", IsRequired = false, DefaultValue = "String")]
        public string Type
        {
            get
            {
                var type = (string)this["Type"];
                if (string.IsNullOrWhiteSpace(type))
                {
                    return "fulltext";
                }
                return type;
            }
            set
            {
                this["Type"] = value;
            }
        }

        public override bool Equals(object compareTo)
        {
            var to = compareTo as IndexField;
            if (to != null)
            {
                return this.Name.Equals(to.Name);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
        
    }
}
