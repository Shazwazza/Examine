using System;
using System.Collections.Generic;
namespace Examine
{
    /// <summary>
    /// Represents all fields and information to index
    /// </summary>
    public interface IIndexCriteria
    {
        /// <summary>
        /// A list of all node types to exclude from the index
        /// </summary>
        //TODO: Do we want these?
        IEnumerable<string> ExcludeNodeTypes { get; }

        /// <summary>
        /// A list of all node types to include in the index
        /// </summary>
        IEnumerable<string> IncludeNodeTypes { get; }
        
        /// <summary>
        /// The starting node id of the node to idnex
        /// </summary>
        //TODO: Remove this
        int? ParentNodeId { get; }

        /// <summary>
        /// A list of the 'standard' attribute fields to index
        /// </summary>
        //TODO: Consolidate these
        IEnumerable<IIndexField> StandardFields { get; }

        /// <summary>
        /// A list of the 'user'/custom fields to index
        /// </summary>
        IEnumerable<IIndexField> UserFields { get; }
    }
}
