using System;
using System.Collections.Generic;
namespace UmbracoExamine.Core
{
    public interface IIndexCriteria
    {
        IEnumerable<string> ExcludeNodeTypes { get; }
        IEnumerable<string> IncludeNodeTypes { get; }
        int? ParentNodeId { get; }
        IEnumerable<string> UmbracoFields { get; }
        IEnumerable<string> UserFields { get; }
    }
}
