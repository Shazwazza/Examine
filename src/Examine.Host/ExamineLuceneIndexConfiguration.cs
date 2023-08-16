using System.Collections.Generic;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Facet;

namespace Examine
{
    /// <summary>
    /// Examine Lucene Index Configuration
    /// </summary>
    /// <typeparam name="TIndex"></typeparam>
    /// <typeparam name="TDirectoryFactory"></typeparam>
    public class ExamineLuceneIndexConfiguration<TIndex, TDirectoryFactory>
            where TIndex : LuceneIndex
            where TDirectoryFactory : class, IDirectoryFactory
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Index Name</param>
        public ExamineLuceneIndexConfiguration(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Index Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Index Field Definitions
        /// </summary>
        public FieldDefinitionCollection? FieldDefinitions { get; set; }

        /// <summary>
        /// Index Default Analyzer
        /// </summary>
        public Analyzer? Analyzer { get; set; }

        /// <summary>
        /// Index Value Set Validator
        /// </summary>
        public IValueSetValidator? Validator { get; set; }

        /// <summary>
        /// Index Value Type Factory
        /// </summary>
        public IReadOnlyDictionary<string, IFieldValueTypeFactory>? IndexValueTypesFactory { get; set; }

        /// <summary>
        /// Index Facet Config
        /// </summary>
        public FacetsConfig? FacetsConfig { get; set; }

        /// <summary>
        /// Whether to use Taxonomy Index
        /// </summary>
        public bool UseTaxonomyIndex { get; set; }

        /// <summary>
        /// The similarties for the <see cref="IIndex"/>
        /// </summary>
        public SimilarityDefinitionCollection? SimilarityDefinitions { get; set; }
    }
}
