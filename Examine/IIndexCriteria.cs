using System;
using System.Collections.Generic;
namespace Examine
{
    public interface IIndexCriteria
    {
        IEnumerable<string> ExcludeNodeTypes { get; }
        IEnumerable<string> IncludeNodeTypes { get; }
        int? ParentNodeId { get; }
        IEnumerable<string> StandardFields { get; }
        IEnumerable<string> UserFields { get; }
    }
}
