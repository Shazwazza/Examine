using System;
using System.Collections.Generic;
using Lucene.Net.Index;
using Lucene.Net.Search;
using UmbracoExamine.Core;
using UmbracoExamine.Core.SearchCriteria;

namespace UmbracoExamine.Providers.SearchCriteria
{
    public class LuceneSearchCriteria : ISearchCriteria
    {
        internal BooleanQuery query;

        internal LuceneSearchCriteria(int max, IndexType type)
        {
            MaxResults = max;
            SearchIndexType = type;
            query = new BooleanQuery();
        }

        private static void ValidateIExamineValue(IExamineValue v)
        {
            var ev = v as ExamineValue;
            if (ev == null)
            {
                throw new ArgumentException("IExamineValue was not created from this provider. Ensure that it is created from the ISearchCriteria this provider exposes");
            }
        }

        #region ISearchCriteria Members

        public int MaxResults
        {
            get;
            private set;
        }

        public IndexType SearchIndexType
        {
            get;
            private set;
        }

        #endregion

        #region ISearch Members

        public IBooleanOperation Id(int id)
        {
            return IdInternal(id, BooleanClause.Occur.SHOULD);
        }

        internal protected IBooleanOperation IdInternal(int id, BooleanClause.Occur occurance)
        {
            query.Add(new TermQuery(new Term(LuceneExamineIndexer.IndexNodeIdFieldName, id.ToString())), occurance);

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation NodeName(string nodeName)
        {
            return NodeName(new ExamineValue(Examineness.Explicit, nodeName));
        }

        public IBooleanOperation NodeName(IExamineValue nodeName)
        {
            return this.NodeNameInternal(nodeName, BooleanClause.Occur.SHOULD);
        }

        internal protected IBooleanOperation NodeNameInternal(IExamineValue ev, BooleanClause.Occur occurance)
        {
            switch (ev.Examineness)
            {
                case Examineness.Fuzzy:
                    query.Add(new FuzzyQuery(new Term("nodeName", ev.Value)), occurance);
                    break;
                case Examineness.SimpleWildcard:
                case Examineness.ComplexWildcard:
                    query.Add(new WildcardQuery(new Term("nodeName", ev.Value)), occurance);
                    break;
                case Examineness.Explicit:
                default:
                    query.Add(new TermQuery(new Term("nodeName", ev.Value)), occurance);
                    break;
            }

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation NodeTypeAlias(string nodeTypeAlias)
        {
            return this.NodeTypeAlias(new ExamineValue(Examineness.Explicit, nodeTypeAlias));
        }

        public IBooleanOperation NodeTypeAlias(IExamineValue nodeTypeAlias)
        {
            return this.NodeTypeAliasInternal(nodeTypeAlias, BooleanClause.Occur.SHOULD);
        }

        internal protected IBooleanOperation NodeTypeAliasInternal(IExamineValue examineValue, BooleanClause.Occur occurance)
        {
            switch (examineValue.Examineness)
            {
                case Examineness.Fuzzy:
                    query.Add(new FuzzyQuery(new Term("nodeTypeAlias", examineValue.Value)), occurance);
                    break;
                case Examineness.SimpleWildcard:
                case Examineness.ComplexWildcard:
                    query.Add(new WildcardQuery(new Term("nodeTypeAlias", examineValue.Value)), occurance);
                    break;
                case Examineness.Explicit:
                default:
                    query.Add(new TermQuery(new Term("nodeTypeAlias", examineValue.Value)), occurance);
                    break;
            }
            
            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation ParentId(int id)
        {
            return this.ParentIdInternal(id, BooleanClause.Occur.SHOULD);
        }

        internal protected IBooleanOperation ParentIdInternal(int id, BooleanClause.Occur occurance)
        {
            query.Add(new TermQuery(new Term("parentID", id.ToString())), occurance);

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation Field(string fieldName, string fieldValue)
        {
            return this.FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), BooleanClause.Occur.SHOULD);
        }

        public IBooleanOperation Field(string fieldName, IExamineValue fieldValue)
        {
            return this.FieldInternal(fieldName, fieldValue, BooleanClause.Occur.SHOULD); 
        }

        internal protected IBooleanOperation FieldInternal(string fieldName, IExamineValue fieldValue, BooleanClause.Occur occurance)
        {
            switch (fieldValue.Examineness)
            {
                case Examineness.Fuzzy:
                    query.Add(new FuzzyQuery(new Term(fieldName, fieldValue.Value)), occurance);
                    break;
                case Examineness.SimpleWildcard:
                case Examineness.ComplexWildcard:
                    query.Add(new WildcardQuery(new Term(fieldName, fieldValue.Value)), occurance);
                    break;
                case Examineness.Explicit:
                default:
                    query.Add(new TermQuery(new Term(fieldName, fieldValue.Value)), occurance);
                    break;
            }

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end)
        {
            return this.Range(fieldName, start, end, true);
        }

        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end, bool inclusive)
        {
            return this.RangeInternal(fieldName, start.ToString("yyyyMMdd"), end.ToString("yyyyMMdd"), inclusive, BooleanClause.Occur.SHOULD);
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
            return this.RangeInternal(fieldName, start, end, inclusive, BooleanClause.Occur.SHOULD);
        }

        protected internal IBooleanOperation RangeInternal(string fieldName, string start, string end, bool inclusive, BooleanClause.Occur occurance)
        {
            query.Add(new RangeQuery(new Term(fieldName, start), new Term(fieldName, end), inclusive), occurance);

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation MultipleFields(IEnumerable<string> fieldNames, string fieldValue)
        {
            foreach (var fieldName in fieldNames)
            {
                this.Field(fieldName, fieldValue);
            }

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation MultipleFields(IEnumerable<string> fieldNames, IExamineValue fieldValue)
        {
            return this.MultipleFieldsInternal(fieldNames, fieldValue, BooleanClause.Occur.SHOULD);
        }

        #endregion


        internal IBooleanOperation MultipleFieldsInternal(IEnumerable<string> fieldNames, IExamineValue fieldValue, BooleanClause.Occur occurance)
        {
            foreach (var fieldName in fieldNames)
            {
                this.FieldInternal(fieldName, fieldValue, occurance);
            }

            return new LuceneBooleanOperation(this);
        }
    }
}
