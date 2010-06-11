using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.SearchCriteria;
using Lucene.Net.Search;

namespace UmbracoExamine.SearchCriteria
{
    public class LuceneBooleanOperation : IBooleanOperation
    {
        private LuceneSearchCriteria search;

        internal LuceneBooleanOperation(LuceneSearchCriteria search)
        {
            this.search = search;
        }

        #region IBooleanOperation Members

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
            if (this.search.SearchIndexType != Examine.IndexType.Any)
            {
                this.search.FieldInternal(LuceneExamineIndexer.IndexTypeFieldName, new ExamineValue(Examineness.Explicit, this.search.SearchIndexType.ToString().ToLower()), BooleanClause.Occur.MUST);
            }
            
            return this.search;
        }

        #endregion
    }
}
