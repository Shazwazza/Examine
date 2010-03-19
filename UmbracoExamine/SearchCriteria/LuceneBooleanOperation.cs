using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoExamine.Core.SearchCriteria;
using Lucene.Net.Search;

namespace UmbracoExamine.Providers.SearchCriteria
{
    public class LuceneBooleanOperation : IBooleanOperation
    {
        private LuceneSearchCriteria search;

        internal LuceneBooleanOperation(LuceneSearchCriteria search)
        {
            this.search = search;
        }

        #region IQuery Members

        public IQuery And()
        {
            return new LuceneQuery(this.search, BooleanClause.Occur.MUST);
        }

        public IQuery Or()
        {
            return new LuceneQuery(this.search, BooleanClause.Occur.SHOULD);
        }

        public IQuery Not()
        {
            return new LuceneQuery(this.search, BooleanClause.Occur.MUST_NOT);
        }

        public ISearchCriteria Compile()
        {
            return this.search;
        }

        #endregion
    }
}
