using System;
using Examine;
using Examine.LuceneEngine;

namespace UmbracoExamine
{
    internal class StaticField : IIndexField
    {
        public StaticField(string name, bool enableSorting, string type)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentNullException("type");

            Type = type;
            EnableSorting = enableSorting;
            Name = name;
        }

        public StaticField(string name, bool enableSorting)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

            Type = "fulltext";
            EnableSorting = enableSorting;
            Name = name;
        }

        [Obsolete("Use another constructor that does not specify an IndexType which is no longer used")]
        public StaticField(string name, FieldIndexTypes indexType, bool enableSorting, string type)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentNullException("type");
            
            Type = type;
            EnableSorting = enableSorting;
            IndexType = indexType;
            Name = name;
        }

        [Obsolete("Use another constructor that does not specify an IndexType which is no longer used")]
        public StaticField(string name, FieldIndexTypes indexType, bool enableSorting)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

            Type = "fulltext";
            EnableSorting = enableSorting;
            IndexType = indexType;
            Name = name;
        }

        public string Name { get; set; }
        public string IndexName { get; set; }

        [Obsolete("This is no longer used")]
        public FieldIndexTypes IndexType { get; private set; }

        public bool EnableSorting { get; set; }
        public string Type { get; set; }
    }
}