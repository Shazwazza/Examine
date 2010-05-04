using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Examine;
using Examine.SearchCriteria;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace UmbracoExamine.SearchCriteria
{
    /// <summary>
    /// 
    /// </summary>
    public class LuceneSearchCriteria : ISearchCriteria
    {
        internal MultiFieldQueryParser queryParser;
        internal BooleanQuery query;
        internal List<SortField> sortFields = new List<SortField>();
        private readonly BooleanClause.Occur occurance;
        private readonly Lucene.Net.Util.Version luceneVersion = Lucene.Net.Util.Version.LUCENE_29;

        internal LuceneSearchCriteria(IndexType type, Analyzer analyzer, string[] fields, BooleanOperation occurance)
        {
            Enforcer.ArgumentNotNull(fields, "fields");

            SearchIndexType = type;
            query = new BooleanQuery();
            this.BooleanOperation = occurance;
            this.queryParser = new MultiFieldQueryParser(luceneVersion, fields, analyzer);
            this.occurance = occurance.ToLuceneOccurance();
        }

        /// <summary>
        /// Gets the boolean operation which this query method will be added as
        /// </summary>
        /// <value>The boolean operation.</value>
        public BooleanOperation BooleanOperation
        {
            get;
            protected set;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{{ MaxResults: {0}, SearchIndexType: {1}, LuceneQuery: {2} }}", this.MaxResults, this.SearchIndexType, this.query.ToString());
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
            protected set;
        }

        public IndexType SearchIndexType
        {
            get;
            protected set;
        }

        public bool IncludeHitCount
        {
            get;
            set;
        }

        public int TotalHits
        {
            get;
            internal protected set;
        }

        #endregion

        #region ISearch Members

        /// <summary>
        /// Query on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public IBooleanOperation Id(int id)
        {
            return IdInternal(id, occurance);
        }

        internal protected IBooleanOperation IdInternal(int id, BooleanClause.Occur occurance)
        {
            //use a query parser (which uses the analyzer) to build up the field query which we want
            query.Add(this.queryParser.GetFieldQuery(LuceneExamineIndexer.IndexNodeIdFieldName, id.ToString()), occurance);

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation NodeName(string nodeName)
        {
            Enforcer.ArgumentNotNull(nodeName, "nodeName");
            return NodeName(new ExamineValue(Examineness.Explicit, nodeName));
        }

        public IBooleanOperation NodeName(IExamineValue nodeName)
        {
            Enforcer.ArgumentNotNull(nodeName, "nodeName");
            return this.NodeNameInternal(nodeName, occurance);
        }

        internal protected IBooleanOperation NodeNameInternal(IExamineValue examineValue, BooleanClause.Occur occurance)
        {
            return this.FieldInternal("nodeName", examineValue, occurance);
        }

        public IBooleanOperation NodeTypeAlias(string nodeTypeAlias)
        {
            Enforcer.ArgumentNotNull(nodeTypeAlias, "nodeTypeAlias");
            return this.NodeTypeAlias(new ExamineValue(Examineness.Explicit, nodeTypeAlias));
        }

        public IBooleanOperation NodeTypeAlias(IExamineValue nodeTypeAlias)
        {
            Enforcer.ArgumentNotNull(nodeTypeAlias, "nodeTypeAlias");
            return this.NodeTypeAliasInternal(nodeTypeAlias, occurance);
        }

        internal protected IBooleanOperation NodeTypeAliasInternal(IExamineValue examineValue, BooleanClause.Occur occurance)
        {
            return this.FieldInternal("nodeTypeAlias", examineValue, occurance);
        }

        public IBooleanOperation ParentId(int id)
        {
            return this.ParentIdInternal(id, occurance);
        }

        internal protected IBooleanOperation ParentIdInternal(int id, BooleanClause.Occur occurance)
        {
            query.Add(this.queryParser.GetFieldQuery("parentID", id.ToString()), occurance);

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation Field(string fieldName, string fieldValue)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            Enforcer.ArgumentNotNull(fieldValue, "fieldValue");
            return this.FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), occurance);
        }

        public IBooleanOperation Field(string fieldName, IExamineValue fieldValue)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            Enforcer.ArgumentNotNull(fieldValue, "fieldValue");
            return this.FieldInternal(fieldName, fieldValue, occurance);
        }

        internal protected IBooleanOperation FieldInternal(string fieldName, IExamineValue fieldValue, BooleanClause.Occur occurance)
        {
            switch (fieldValue.Examineness)
            {
                case Examineness.Fuzzy:
                    query.Add(this.queryParser.GetFuzzyQuery(fieldName, fieldValue.Value, fieldValue.Level), occurance);
                    break;
                case Examineness.SimpleWildcard:
                case Examineness.ComplexWildcard:
                    query.Add(this.queryParser.GetWildcardQuery(fieldName, fieldValue.Value), occurance);
                    break;
                case Examineness.Explicit:
                default:
                    query.Add(this.queryParser.GetFieldQuery(fieldName, fieldValue.Value), occurance);
                    break;
            }

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end)
        {
            return this.Range(fieldName, start, end, true, true);
        }

        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end, bool includeLower, bool includeUpper)
        {
            //since lucene works on string's for all searching we need to flatten the date
            return this.RangeInternal(fieldName, start.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture), end.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture), includeLower, includeUpper, occurance);
        }

        public IBooleanOperation Range(string fieldName, int start, int end)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            return this.Range(fieldName, start, end, true, true);
        }

        public IBooleanOperation Range(string fieldName, int start, int end, bool includeLower, bool includeUpper)
        {
            return this.RangeInternal(fieldName, start, end, includeLower, includeUpper, occurance);
        }

        protected internal IBooleanOperation RangeInternal(string fieldName, int start, int end, bool includeLower, bool includeUpper, BooleanClause.Occur occurance)
        {
            query.Add(NumericRangeQuery.NewIntRange(fieldName, start, end, includeLower, includeUpper), occurance);
            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation Range(string fieldName, string start, string end)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            Enforcer.ArgumentNotNull(start, "start");
            Enforcer.ArgumentNotNull(end, "end");
            return this.Range(fieldName, start, end, true, true);
        }

        public IBooleanOperation Range(string fieldName, string start, string end, bool includeLower, bool includeUpper)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            Enforcer.ArgumentNotNull(start, "start");
            Enforcer.ArgumentNotNull(end, "end");
            return this.RangeInternal(fieldName, start, end, includeLower, includeUpper, occurance);
        }

        protected internal IBooleanOperation RangeInternal(string fieldName, string start, string end, bool includeLower, bool includeUpper, BooleanClause.Occur occurance)
        {
            query.Add(new TermRangeQuery(fieldName, start, end, includeLower, includeUpper), occurance);

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(query, "query");
            return this.GroupedAndInternal(fields.ToArray(), query, occurance);
        }

        protected internal IBooleanOperation GroupedAndInternal(string[] fields, string[] query, BooleanClause.Occur occurance)
        {
            var flags = new BooleanClause.Occur[fields.Length];
            for (int i = 0; i < flags.Length; i++)
                flags[i] = BooleanClause.Occur.MUST;

            //if there's only 1 query text we want to build up a string like this:
            //(+field1:query +field2:query +field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)
            if (query.Length > 1)
                this.query.Add(MultiFieldQueryParser.Parse(luceneVersion, query, fields.ToArray(), flags, this.queryParser.GetAnalyzer()), occurance);
            else
                this.query.Add(MultiFieldQueryParser.Parse(luceneVersion, query[0], fields.ToArray(), flags, this.queryParser.GetAnalyzer()), occurance);

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(query, "query");

            return this.GroupedOrInternal(fields.ToArray(), query, occurance);
        }

        protected internal IBooleanOperation GroupedOrInternal(string[] fields, string[] query, BooleanClause.Occur occurance)
        {
            //need to have the same number of occurrence flags as fields
            var flags = new BooleanClause.Occur[fields.Length];
            for (int i = 0; i < flags.Length; i++)
                flags[i] = BooleanClause.Occur.SHOULD;

            //if there's only 1 query text we want to build up a string like this:
            //(field1:query field2:query field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)
            if (query.Length > 1)
                this.query.Add(MultiFieldQueryParser.Parse(luceneVersion, query, fields.ToArray(), flags, this.queryParser.GetAnalyzer()), occurance);
            else
                this.query.Add(MultiFieldQueryParser.Parse(luceneVersion, query[0], fields.ToArray(), flags, this.queryParser.GetAnalyzer()), occurance);

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(query, "query");

            return this.GroupedNotInternal(fields.ToArray(), query, occurance);
        }

        protected internal IBooleanOperation GroupedNotInternal(string[] fields, string[] query, BooleanClause.Occur occurance)
        {
            var flags = new BooleanClause.Occur[fields.Length];
            for (int i = 0; i < flags.Length; i++)
                flags[i] = BooleanClause.Occur.MUST_NOT;

            //if there's only 1 query text we want to build up a string like this:
            //(!field1:query !field2:query !field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)
            if (query.Length > 1)
                this.query.Add(MultiFieldQueryParser.Parse(luceneVersion, query, fields.ToArray(), flags, this.queryParser.GetAnalyzer()), occurance);
            else
                this.query.Add(MultiFieldQueryParser.Parse(luceneVersion, query[0], fields.ToArray(), flags, this.queryParser.GetAnalyzer()), occurance);

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params string[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(query, "query");
            Enforcer.ArgumentNotNull(operations, "operations");

            return this.GroupedFlexibleInternal(fields.ToArray(), operations.ToArray(), query, occurance);
        }

        protected internal IBooleanOperation GroupedFlexibleInternal(string[] fields, BooleanOperation[] operations, string[] query, BooleanClause.Occur occurance)
        {
            var flags = new BooleanClause.Occur[operations.Count()];
            for (int i = 0; i < flags.Length; i++)
                flags[i] = operations.ElementAt(i).ToLuceneOccurance();

            //if there's only 1 query text we want to build up a string like this:
            //(field1:query field2:query field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)
            if (query.Length > 1)
                this.query.Add(MultiFieldQueryParser.Parse(luceneVersion, query, fields, flags, this.queryParser.GetAnalyzer()), occurance);
            else
                this.query.Add(MultiFieldQueryParser.Parse(luceneVersion, query[0], fields, flags, this.queryParser.GetAnalyzer()), occurance);

            return new LuceneBooleanOperation(this);
        }

        /// <summary>
        /// Passes a raw search query to the provider to handle
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public ISearchCriteria RawQuery(string query)
        {
            this.query.Add(this.queryParser.Parse(query), this.occurance);

            return this;
        }

        /// <summary>
        /// Orders the results by the specified fields
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns></returns>
        public IBooleanOperation OrderBy(params string[] fieldNames)
        {
            Enforcer.ArgumentNotNull(fieldNames, "fieldNames");

            return this.OrderByInternal(false, fieldNames);
        }

        /// <summary>
        /// Orders the results by the specified fields in a descending order
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns></returns>
        public IBooleanOperation OrderByDescending(params string[] fieldNames)
        {
            Enforcer.ArgumentNotNull(fieldNames, "fieldNames");

            return this.OrderByInternal(true, fieldNames);
        }

        /// <summary>
        /// Internal operation for adding the ordered results
        /// </summary>
        /// <param name="descending">if set to <c>true</c> [descending].</param>
        /// <param name="fieldNames">The field names.</param>
        /// <returns></returns>
        protected internal IBooleanOperation OrderByInternal(bool descending, params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                this.sortFields.Add(new SortField(LuceneExamineIndexer.SortedFieldNamePrefix + fieldName, SortField.STRING, descending));
            }

            return new LuceneBooleanOperation(this);
        }

        #endregion
    }
}
