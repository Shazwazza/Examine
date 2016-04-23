using System;

namespace Examine
{    
    /// <summary>
    /// Defines a field to be indexed
    /// </summary>
    public class FieldDefinition : IEquatable<FieldDefinition>
    {
        private readonly string _indexName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public FieldDefinition(string name, string type)
        {
            Name = name;
            Type = type;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="indexName"></param>
        /// <param name="type"></param>
        public FieldDefinition(string name, string indexName, string type)
        {
            _indexName = indexName;
            Name = name;
            Type = type;
        }

        /// <summary>
        /// The name of the index field
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The data type
        /// </summary>
        public string Type { get; private set; }

        //TODO: REname this to something more relavent once we figure out exactly what it is doing!
        //TODO: Test this !!
        /// <summary>
        /// IndexName is so that you can index the same field with different analyzers
        /// </summary>
        /// <remarks>
        /// You might for instance both use a prefix indexer and a full text indexer. Also, if you have multiple data sources you can use a common field name in the index. 
        /// If it's not specified it will just be the field name. 
        /// If this is null it should default to 'Name'
        /// </remarks>
        public string IndexName
        {
            get { return _indexName ?? Name; }
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(FieldDefinition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(_indexName, other._indexName) && string.Equals(Name, other.Name) && string.Equals(Type, other.Type);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FieldDefinition) obj);
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_indexName != null ? _indexName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Type != null ? Type.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(FieldDefinition left, FieldDefinition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FieldDefinition left, FieldDefinition right)
        {
            return !Equals(left, right);
        }
    }
}