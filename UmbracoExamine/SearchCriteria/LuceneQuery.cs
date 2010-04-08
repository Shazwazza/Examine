using System;
using System.Linq;
using System.Collections.Generic;
using Lucene.Net.Search;
using Examine.SearchCriteria;

namespace UmbracoExamine.SearchCriteria
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

        public BooleanOperation BooleanOperation
        {
            get { return occurance.ToBooleanOperation(); }
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
            return this.Range(fieldName, start, end, true, true);
        }

        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end, bool includeLower, bool includeUpper)
        {
            return this.search.Range(fieldName, start, end, includeLower, includeUpper);
        }

        public IBooleanOperation Range(string fieldName, int start, int end)
        {
            return this.Range(fieldName, start, end, true, true);
        }

        public IBooleanOperation Range(string fieldName, int start, int end, bool includeLower, bool includeUpper)
        {
            return this.search.RangeInternal(fieldName, start, end, includeLower, includeUpper, occurance);
        }

        public IBooleanOperation Range(string fieldName, string start, string end)
        {
            return this.Range(fieldName, start, end, true, true);
        }
  
        public IBooleanOperation Range(string fieldName, string start, string end, bool includeLower, bool includeUpper)
        {
            return this.search.RangeInternal(fieldName, start, end, includeLower, includeUpper, occurance);
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
        
        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query)
        {
            return this.search.GroupedAndInternal(fields.ToArray(), query, this.occurance);
        }

        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query)
        {
            return this.search.GroupedOrInternal(fields.ToArray(), query, this.occurance);
        }

        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query)
        {
            return this.search.GroupedNotInternal(fields.ToArray(), query, this.occurance);
        }

        public IBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params string[] query)
        {
            return this.search.GroupedFlexibleInternal(fields.ToArray(), operations.ToArray(), query, occurance);
        }

        #endregion
    }
}
