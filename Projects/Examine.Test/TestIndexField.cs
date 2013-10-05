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

        private string _indexName;
        public string Name { get; set; }
        
        public string IndexName
        {
            get { return _indexName ?? Name; }
            set { _indexName = value; }
        }

        public bool EnableSorting { get; set; }
        public string Type { get; set; }
    }
}
