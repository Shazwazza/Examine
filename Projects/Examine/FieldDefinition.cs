namespace Examine
{    
    /// <summary>
    /// Defines a field to be indexed
    /// </summary>
    public class FieldDefinition
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
    }
}