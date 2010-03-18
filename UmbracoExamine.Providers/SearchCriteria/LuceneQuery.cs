using System;
using System.Collections.Generic;
using Lucene.Net.Search;
using UmbracoExamine.Core.SearchCriteria;

namespace UmbracoExamine.Providers.SearchCriteria
{
    public class LuceneQuery : IQuery
    {
        private LuceneSearchCriteria search;
        private BooleanClause.Occur occurance;

        internal LuceneQuery(LuceneSearchCriteria search, BooleanClause.Occur occurance)
        {
            this.search = search;
            this.occurance = occurance;
        }

        #region ISearch Members

        public IBooleanOperation Id(int id)
        {
            return this.search.IdInternal(id, this.occurance);
        }

        public IBooleanOperation NodeName(string nodeName)
        {
            return this.search.NodeNameInternal(new ExamineValue(Examineness.Explicit, nodeName), occurance);
        }

        public IBooleanOperation NodeTypeAlias(string nodeTypeAlias)
        {
            return this.search.NodeTypeAliasInternal(new ExamineValue(Examineness.Explicit, nodeTypeAlias), occurance);
        }

        public IBooleanOperation ParentId(int id)
        {
            return this.search.ParentIdInternal(id, occurance);
        }

        public IBooleanOperation Field(string fieldName, string fieldValue)
        {
            return this.search.FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), occurance);
        }

        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end)
        {
            return this.Range(fieldName, start, end, true);
        }

        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end, bool inclusive)
        {
            return this.Range(fieldName, start.ToString("yyyyMMdd"), end.ToString("yyyyMMdd"), inclusive);
        }

        public IBooleanOperation Range(string fieldName, int start, int end)
        {
            return this.Range(fieldName, start, end, true);
        }

        public IBooleanOperation Range(string fieldName, int start, int end, bool inclusive)
        {
            return this.Range(fieldName, start.ToString(), end.ToString(), inclusive);
        }

        public IBooleanOperation Range(string fieldName, string start, string end)
        {
            return this.Range(fieldName, start, end, true);
        }
  
        public IBooleanOperation Range(string fieldName, string start, string end, bool inclusive)
        {
            return this.search.RangeInternal(fieldName, start, end, inclusive, occurance);
        }

        public ISearchCriteria Compile()
        {
            return search;
        }

        public IBooleanOperation NodeName(IExamineValue nodeName)
        {
            return this.search.NodeNameInternal(nodeName, occurance);
        }

        public IBooleanOperation NodeTypeAlias(IExamineValue nodeTypeAlias)
        {
            return this.search.NodeTypeAliasInternal(nodeTypeAlias, occurance);
        }

        public IBooleanOperation Field(string fieldName, IExamineValue fieldValue)
        {
            return this.search.FieldInternal(fieldName, fieldValue, occurance);
        }
        
        public IBooleanOperation MultipleFields(IEnumerable<string> fieldNames, string fieldValue)
        {
            return this.MultipleFields(fieldNames, new ExamineValue(Examineness.Explicit, fieldValue));
        }

        public IBooleanOperation MultipleFields(IEnumerable<string> fieldNames, IExamineValue fieldValue)
        {
            return this.search.MultipleFieldsInternal(fieldNames, fieldValue, occurance);
        }


        #endregion

    }
}
