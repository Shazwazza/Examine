using System;
using System.Collections.Generic;

namespace Examine.SearchCriteria
{
    public interface ISearchCriteria : IQuery
    {
        int MaxResults { get; }
        IndexType SearchIndexType { get; }
    }
}
