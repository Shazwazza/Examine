using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Examine.Lucene;
using Examine.Lucene.Analyzers;
using Examine.Lucene.Directories;
using Examine.Lucene.Indexing;
using Lucene.Net.Index;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Examine.Web.Demo
{
    /// <summary>
    /// Configure Examine indexes using .NET IOptions
    /// </summary>
    public sealed class ConfigureIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
    {
        private readonly ILoggerFactory _loggerFactory;

        public ConfigureIndexOptions(ILoggerFactory loggerFactory)
            => _loggerFactory = loggerFactory;

        public void Configure(string name, LuceneDirectoryIndexOptions options)
        {
            switch (name)
            {
                case "MyIndex":
                    // Create a dictionary for custom value types.
                    // They keys are the value type names.
                    options.IndexValueTypesFactory = new Dictionary<string, IFieldValueTypeFactory>
                    {
                        // Create a phone number value type using the GenericAnalyzerFieldValueType
                        // to pass in a custom analyzer. As an example, it could use Examine's
                        // PatternAnalyzer to pass in a phone number pattern to match.
                        ["phone"] = new DelegateFieldValueTypeFactory(name =>
                                        new GenericAnalyzerFieldValueType(
                                            name,
                                            _loggerFactory,
                                            new PatternAnalyzer(@"\d{3}\s\d{3}\s\d{4}", 0)))
                    };

                    // Add the field definition for a field called "phone" which maps
                    // to a Value Type called "phone" defined above.
                    options.FieldDefinitions.AddOrUpdate(new FieldDefinition("phone", "phone"));
                    break;
            }
        }

        public void Configure(LuceneDirectoryIndexOptions options)
            => throw new NotImplementedException("This is never called and is just part of the interface");
    }


}
