using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using Examine.LuceneEngine.Faceting;

namespace Examine
{
    /// <summary>
    /// a data structure for storing indexing/searching instructions
    /// </summary>
    [Obsolete("This shouldn't be used anymore but exists for legacy reasons")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IndexCriteria : IIndexCriteria
    {

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="standardFields"></param>
        ///<param name="userFields"></param>
        ///<param name="includeNodeTypes"></param>
        ///<param name="excludeNodeTypes"></param>
        ///<param name="parentNodeId"></param>
        public IndexCriteria(IEnumerable<IIndexField> standardFields, IEnumerable<IIndexField> userFields, 
            IEnumerable<string> includeNodeTypes, IEnumerable<string> excludeNodeTypes, int? parentNodeId)
        {
            UserFields = userFields.ToList();
            StandardFields = standardFields.ToList();
            IncludeNodeTypes = includeNodeTypes;
            ExcludeNodeTypes = excludeNodeTypes;
            ParentNodeId = parentNodeId;
        }

        public IEnumerable<IIndexField> StandardFields { get; internal set; }
        public IEnumerable<IIndexField> UserFields { get; internal set; }
        public FacetConfiguration FacetConfiguration { get; internal set; }

        public IEnumerable<string> IncludeNodeTypes { get; internal set; }
        public IEnumerable<string> ExcludeNodeTypes { get; internal set; }
        public int? ParentNodeId { get; internal set; }
    }

    
}
