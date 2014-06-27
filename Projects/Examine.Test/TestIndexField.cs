using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.Test
{
    public class TestIndexField : IIndexField
    {
        public TestIndexField(string name, string type, bool enableSorting = false)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentNullException("type");
            
            Type = type;
            EnableSorting = enableSorting;
            Name = name;
        }

        public TestIndexField(string name, bool enableSorting = false)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            
            Type = "fulltext";
            EnableSorting = enableSorting;
            Name = name;
        }
    
        public string Name { get; set; }
        public bool EnableSorting { get; set; }
        public string Type { get; set; }
    }
}
