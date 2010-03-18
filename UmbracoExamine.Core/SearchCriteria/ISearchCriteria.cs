using System;
using System.Collections.Generic;

namespace UmbracoExamine.Core.SearchCriteria
{
    public interface ISearchCriteria : IQuery
    {
        int MaxResults { get; }
        IndexType SearchIndexType { get; }
    }
}
