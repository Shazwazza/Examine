using System;
using System.Collections.Generic;
namespace UmbracoExamine.Core
{
    public interface IIndexCriteria
    {
        string[] ExcludeNodeTypes { get; }
        string[] IncludeNodeTypes { get; }
        int? ParentNodeId { get; }
        IEnumerable<string> UmbracoFields { get; }
        IEnumerable<string> UserFields { get; }
    }
}
