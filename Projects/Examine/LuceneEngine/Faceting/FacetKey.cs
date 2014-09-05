using System;
using System.Diagnostics.Contracts;

namespace Examine.LuceneEngine.Faceting
{
    public class FacetKey : IComparable<FacetKey>
    {
        public string FieldName { get; private set; }

        public string Value { get; private set; }

        public FacetKey(string fieldName, string value)
        {
            if (fieldName == null) throw new ArgumentNullException("fieldName");
            if (value == null) throw new ArgumentNullException("value");
            
            FieldName = fieldName;
            Value = value;
        }

        public FacetKey Intern()
        {
            FieldName = string.Intern(FieldName);
            Value = string.Intern(Value);

            return this;
        }

        public int CompareTo(FacetKey other)
        {
            var c = FieldName.CompareTo(other.FieldName);
            return c == 0 ? Value.CompareTo(other.Value) : c;
        }

        public override string ToString()
        {
            return (FieldName != "" ? FieldName + ":" : "") + Value;
        }        
        

        public virtual bool Equals(FacetKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.FieldName, FieldName) && Equals(other.Value, Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is FacetKey)) return false;
            return Equals((FacetKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (FieldName.GetHashCode() * 397) ^ Value.GetHashCode();
            }
        }
    }
}
