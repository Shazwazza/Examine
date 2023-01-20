using System;
using System.Linq;

namespace Examine
{
    /// <summary>
    /// Defines a suggester for an Index
    /// </summary>
    public struct SuggesterDefinition : IEquatable<SuggesterDefinition>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="suggesterMode"></param>
        /// <param name="sourceFields"></param>
        public SuggesterDefinition(string name, string suggesterMode, string[] sourceFields)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            if (string.IsNullOrWhiteSpace(suggesterMode))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(suggesterMode));

            Name = name;
            SourceFields = sourceFields;
            SuggesterMode = suggesterMode;
        }

        /// <summary>
        /// The name of the Suggester
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Suggester Source Fields
        /// </summary>
        public string[] SourceFields { get; }

        /// <summary>
        /// The suggester type
        /// </summary>
        public string SuggesterMode { get; }

        public bool Equals(SuggesterDefinition other) => string.Equals(Name, other.Name) && string.Equals(SuggesterMode, other.SuggesterMode);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is SuggesterDefinition definition && Equals(definition);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name.GetHashCode() * 397) ^ SuggesterMode.GetHashCode();
            }
        }

        public static bool operator ==(SuggesterDefinition left, SuggesterDefinition right) => left.Equals(right);

        public static bool operator !=(SuggesterDefinition left, SuggesterDefinition right) => !left.Equals(right);
    }
}
