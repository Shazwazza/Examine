using System;

namespace Examine
{
    /// <summary>
    /// Defines a suggester for an Index
    /// </summary>
    public class SuggesterDefinition
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the suggester</param>
        /// <param name="sourceFields">Source Index Fields for the Suggester</param>
        public SuggesterDefinition(string name, string[] sourceFields = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            Name = name;
            SourceFields = sourceFields;
        }

        /// <summary>
        /// The name of the Suggester
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Suggester Source Fields
        /// </summary>
        public string[] SourceFields { get; }
    }
}
