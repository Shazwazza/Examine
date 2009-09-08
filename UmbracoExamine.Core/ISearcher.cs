using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UmbracoExamine.Core
{
    public interface ISearcher
    {

        IEnumerable<SearchResult> Search(ISearchCriteria criteria);

    }
}
