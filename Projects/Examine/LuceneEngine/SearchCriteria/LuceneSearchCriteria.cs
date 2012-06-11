using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Examine;
using Examine.SearchCriteria;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Search.Spans;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Examine.LuceneEngine.Providers;

namespace Examine.LuceneEngine.SearchCriteria
{
    /// <summary>
    /// This class is used to query against Lucene.Net
    /// </summary>
    public class LuceneSearchCriteria : ISearchCriteria
    {
        internal MultiFieldQueryParser QueryParser;
        internal BooleanQuery Query;
        internal List<SortField> SortFields = new List<SortField>();
        private readonly BooleanClause.Occur _occurance;
        private readonly Lucene.Net.Util.Version _luceneVersion = Lucene.Net.Util.Version.LUCENE_29;

        #region Field Name properties
        private string _NodeTypeAliasField = "nodeTypeAlias";

        /// <summary>
        /// Defines the field name to use for the node type alias query
        /// </summary>
        public string NodeTypeAliasField
        {
            get { return _NodeTypeAliasField; }
            set { _NodeTypeAliasField = value; }
        }

        private string _NodeNameField = "nodeName";

        /// <summary>
        /// Defines the field name to use for the node name query
        /// </summary>
        public string NodeNameField
        {
            get { return _NodeNameField; }
            set { _NodeNameField = value; }
        }

        private string _ParentIdField = "parentID";

        /// <summary>
        /// Defines the field name to use for the parent id query
        /// </summary>
        public string ParentIdField
        {
            get { return _ParentIdField; }
            set { _ParentIdField = value; }
        } 
        #endregion

        internal LuceneSearchCriteria(string type, Analyzer analyzer, string[] fields, bool allowLeadingWildcards, BooleanOperation occurance)
        {
            Enforcer.ArgumentNotNull(fields, "fields");

            SearchIndexType = type;
            Query = new BooleanQuery();
            this.BooleanOperation = occurance;
            this.QueryParser = new MultiFieldQueryParser(_luceneVersion, fields, analyzer);
            this.QueryParser.SetAllowLeadingWildcard(allowLeadingWildcards);
            this._occurance = occurance.ToLuceneOccurance();
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
            return string.Format("{{ SearchIndexType: {0}, LuceneQuery: {1} }}", this.SearchIndexType, this.Query.ToString());
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

        public string SearchIndexType
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
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public IBooleanOperation Id(int id)
        {
            return IdInternal(id, _occurance);
        }

        internal protected IBooleanOperation IdInternal(int id, BooleanClause.Occur occurance)
        {
            //use a query parser (which uses the analyzer) to build up the field query which we want
            Query.Add(this.QueryParser.GetFieldQuery(LuceneIndexer.IndexNodeIdFieldName, id.ToString()), occurance);

            return new LuceneBooleanOperation(this);
        }

        /// <summary>
        /// Query on the NodeName
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public IBooleanOperation NodeName(string nodeName)
        {
            Enforcer.ArgumentNotNull(nodeName, "nodeName");
            return NodeName(new ExamineValue(Examineness.Explicit, nodeName));
        }

        /// <summary>
        /// Query on the NodeName
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public IBooleanOperation NodeName(IExamineValue nodeName)
        {
            Enforcer.ArgumentNotNull(nodeName, "nodeName");
            return this.NodeNameInternal(nodeName, _occurance);
        }

        internal protected IBooleanOperation NodeNameInternal(IExamineValue examineValue, BooleanClause.Occur occurance)
        {
            return this.FieldInternal(NodeNameField, examineValue, occurance);
        }

        /// <summary>
        /// Query on the NodeTypeAlias
        /// </summary>
        /// <param name="nodeTypeAlias">The node type alias.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public IBooleanOperation NodeTypeAlias(string nodeTypeAlias)
        {
            Enforcer.ArgumentNotNull(nodeTypeAlias, "nodeTypeAlias");            
            return this.NodeTypeAlias(new ExamineValue(Examineness.Explicit, nodeTypeAlias));
        }

        /// <summary>
        /// Query on the NodeTypeAlias
        /// </summary>
        /// <param name="nodeTypeAlias">The node type alias.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public IBooleanOperation NodeTypeAlias(IExamineValue nodeTypeAlias)
        {
            Enforcer.ArgumentNotNull(nodeTypeAlias, "nodeTypeAlias");
            return this.NodeTypeAliasInternal(nodeTypeAlias, _occurance);
        }

        internal protected IBooleanOperation NodeTypeAliasInternal(IExamineValue examineValue, BooleanClause.Occur occurance)
        {
            //force lower case
            var eVal = new ExamineValue(examineValue.Examineness, examineValue.Value.ToLower(), examineValue.Level);
            //don't use the query parser for this operation, it needs to match exact
            return this.FieldInternal(NodeTypeAliasField, eVal, occurance, false);
        }

        /// <summary>
        /// Query on the Parent ID
        /// </summary>
        /// <param name="id">The id of the parent.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public IBooleanOperation ParentId(int id)
        {
            return this.ParentIdInternal(id, _occurance);
        }

        internal protected IBooleanOperation ParentIdInternal(int id, BooleanClause.Occur occurance)
        {
            Query.Add(this.QueryParser.GetFieldQuery(ParentIdField, id.ToString()), occurance);

            return new LuceneBooleanOperation(this);
        }

        /// <summary>
        /// Query on the specified field
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public IBooleanOperation Field(string fieldName, string fieldValue)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            Enforcer.ArgumentNotNull(fieldValue, "fieldValue");
            return this.FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), _occurance);
        }

        /// <summary>
        /// Query on the specified field
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public IBooleanOperation Field(string fieldName, IExamineValue fieldValue)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            Enforcer.ArgumentNotNull(fieldValue, "fieldValue");
            return this.FieldInternal(fieldName, fieldValue, _occurance);
        }

        /// <summary>
        /// Returns the Lucene query object for a field given an IExamineValue
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="useQueryParser">True to use the query parser to parse the search text, otherwise, manually create the queries</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        internal protected Query GetFieldInternalQuery(string fieldName, IExamineValue fieldValue, bool useQueryParser)
        {
            Query queryToAdd;

            switch (fieldValue.Examineness)
            {
                case Examineness.Fuzzy:
                    if (useQueryParser)
                    {
                        queryToAdd = this.QueryParser.GetFuzzyQuery(fieldName, fieldValue.Value, fieldValue.Level);
                    }
                    else
                    {
                        //REFERENCE: http://lucene.apache.org/java/2_4_0/queryparsersyntax.html#Fuzzy%20Searches
                        var proxQuery = fieldName + ":" + fieldValue.Value + "~" + Convert.ToInt32(fieldValue.Level).ToString();
                        queryToAdd = ParseRawQuery(proxQuery);
                    }
                    break;
                case Examineness.SimpleWildcard:
                case Examineness.ComplexWildcard:
                    if (useQueryParser)
                    {
                        queryToAdd = this.QueryParser.GetWildcardQuery(fieldName, fieldValue.Value);
                    }
                    else
                    {
                        //this will already have a * or a . suffixed based on the extension methods
                        //REFERENCE: http://lucene.apache.org/java/2_4_0/queryparsersyntax.html#Wildcard%20Searches
                        var proxQuery = fieldName + ":" + fieldValue.Value;
                        queryToAdd = ParseRawQuery(proxQuery);
                    }
                    break;
                case Examineness.Boosted:
                    if (useQueryParser)
                    {
                        queryToAdd = this.QueryParser.GetFieldQuery(fieldName, fieldValue.Value);
                        queryToAdd.SetBoost(fieldValue.Level);
                    }
                    else
                    {
                        //REFERENCE: http://lucene.apache.org/java/2_4_0/queryparsersyntax.html#Boosting%20a%20Term
                        var proxQuery = fieldName + ":\"" + fieldValue.Value + "\"^" + Convert.ToInt32(fieldValue.Level).ToString();
                        queryToAdd = ParseRawQuery(proxQuery);
                    }
                    break;
                case Examineness.Proximity:

                    //This is how you are supposed to do this based on this doc here:
                    //http://lucene.apache.org/java/2_4_1/api/org/apache/lucene/search/spans/package-summary.html#package_description
                    //but i think that lucene.net has an issue with it's internal parser since it parses to a very strange query
                    //we'll just manually make it instead below

                    //var spans = new List<SpanQuery>();
                    //foreach (var s in fieldValue.Value.Split(' '))
                    //{
                    //    spans.Add(new SpanTermQuery(new Term(fieldName, s)));
                    //}
                    //queryToAdd = new SpanNearQuery(spans.ToArray(), Convert.ToInt32(fieldValue.Level), true);

                    var qry = fieldName + ":\"" + fieldValue.Value + "\"~" + Convert.ToInt32(fieldValue.Level).ToString();
                    if (useQueryParser)
                    {                        
                        queryToAdd = QueryParser.Parse(qry); 
                    }
                    else
                    {
                        queryToAdd = ParseRawQuery(qry); 
                    }
                    break;
                case Examineness.Explicit:
                default:
                    if (useQueryParser)
                    {
                        queryToAdd = this.QueryParser.GetFieldQuery(fieldName, fieldValue.Value); 
                    }
                    else
                    {
                        //standard query 
                        var proxQuery = fieldName + ":" + fieldValue.Value;
                        queryToAdd = ParseRawQuery(proxQuery);
                    }
                    break;
            }
            return queryToAdd;
        }

        /// <summary>
        /// This parses a raw query into a non-tokenized query.
        /// not analyzing/tokenizing the search string
        /// </summary>
        /// <remarks>
        /// Currently this is done by just using the keyword analyzer which doesn't parse special chars, whitespace, etc..
        /// however there may be a better way to acheive this, or could manually parse into a boolean query
        /// using TermQueries.
        /// </remarks>
        /// <param name="rawQuery"></param>
        /// <returns></returns>
        internal protected Query ParseRawQuery(string rawQuery)
        {
            var qry = new QueryParser(_luceneVersion, "", new KeywordAnalyzer());
            return qry.Parse(rawQuery);
        }

        internal protected IBooleanOperation FieldInternal(string fieldName, IExamineValue fieldValue, BooleanClause.Occur occurance)
        {
            return FieldInternal(fieldName, fieldValue, occurance, true);
        }

        internal protected IBooleanOperation FieldInternal(string fieldName, IExamineValue fieldValue, BooleanClause.Occur occurance, bool useQueryParser)
        {
            Query queryToAdd = GetFieldInternalQuery(fieldName, fieldValue, useQueryParser);            

            if (queryToAdd != null)
                Query.Add(queryToAdd, occurance);

            return new LuceneBooleanOperation(this);
        }

        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end)
        {
            return this.Range(fieldName, start, end, true, true);
        }

        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="includeLower">if set to <c>true</c> [include lower].</param>
        /// <param name="includeUpper">if set to <c>true</c> [include upper].</param>
        /// <returns></returns>
        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end, bool includeLower, bool includeUpper)
        {
            return this.Range(fieldName, start, end, true, true, DateResolution.Millisecond);
        }

        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="includeLower">if set to <c>true</c> [include lower].</param>
        /// <param name="includeUpper">if set to <c>true</c> [include upper].</param>
        /// <param name="resolution">The resolution.</param>
        /// <returns></returns>
        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end, bool includeLower, bool includeUpper, DateResolution resolution)
        {
            //By specifying the resolution we can do more accurate range searching on date fields
            DateTools.Resolution luceneResolution;
            switch (resolution)
            {
                case DateResolution.Year:
                    luceneResolution = DateTools.Resolution.YEAR;
                    break;
                case DateResolution.Month:
                    luceneResolution = DateTools.Resolution.MONTH;
                    break;
                case DateResolution.Day:
                    luceneResolution = DateTools.Resolution.DAY;
                    break;
                case DateResolution.Hour:
                    luceneResolution = DateTools.Resolution.HOUR;
                    break;
                case DateResolution.Minute:
                    luceneResolution = DateTools.Resolution.MINUTE;
                    break;
                case DateResolution.Second:
                    luceneResolution = DateTools.Resolution.SECOND;
                    break;
                case DateResolution.Millisecond:
                default:
                    luceneResolution = DateTools.Resolution.MILLISECOND;
                    break;
            }
            //since lucene works on string's for all searching we need to flatten the date
            return this.RangeInternal(fieldName, DateTools.DateToString(start, luceneResolution), DateTools.DateToString(end, luceneResolution), includeLower, includeUpper, _occurance);
        }

        public IBooleanOperation Range(string fieldName, int start, int end)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            return this.Range(fieldName, start, end, true, true);
        }

        public IBooleanOperation Range(string fieldName, int start, int end, bool includeLower, bool includeUpper)
        {
            return this.RangeInternal(fieldName, start, end, includeLower, includeUpper, _occurance);
        }

        protected internal IBooleanOperation RangeInternal(string fieldName, int start, int end, bool includeLower, bool includeUpper, BooleanClause.Occur occurance)
        {
            Query.Add(NumericRangeQuery.NewIntRange(fieldName, start, end, includeLower, includeUpper), occurance);
            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation Range(string fieldName, double lower, double upper)
        {
            return this.Range(fieldName, lower, upper, true, true);
        }

        public IBooleanOperation Range(string fieldName, double lower, double upper, bool includeLower, bool includeUpper)
        {
            return this.RangeInternal(fieldName, lower, upper, includeLower, includeUpper, _occurance);
        }

        protected internal IBooleanOperation RangeInternal(string fieldName, double lower, double upper, bool includeLower, bool includeUpper, BooleanClause.Occur occurance)
        {
            Query.Add(NumericRangeQuery.NewDoubleRange(fieldName, lower, upper, includeLower, includeUpper), occurance);
            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation Range(string fieldName, float lower, float upper)
        {
            return this.Range(fieldName, lower, upper, true, true);
        }

        public IBooleanOperation Range(string fieldName, float lower, float upper, bool includeLower, bool includeUpper)
        {
            return this.RangeInternal(fieldName, lower, upper, includeLower, includeUpper, _occurance);
        }

        protected internal IBooleanOperation RangeInternal(string fieldName, float lower, float upper, bool includeLower, bool includeUpper, BooleanClause.Occur occurance)
        {
            Query.Add(NumericRangeQuery.NewFloatRange(fieldName, lower, upper, includeLower, includeUpper), occurance);
            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation Range(string fieldName, long lower, long upper)
        {
            return this.Range(fieldName, lower, upper, true, true);
        }

        public IBooleanOperation Range(string fieldName, long lower, long upper, bool includeLower, bool includeUpper)
        {
            return this.RangeInternal(fieldName, lower, upper, includeLower, includeUpper, _occurance);
        }

        protected internal IBooleanOperation RangeInternal(string fieldName, long lower, long upper, bool includeLower, bool includeUpper, BooleanClause.Occur occurance)
        {
            Query.Add(NumericRangeQuery.NewLongRange(fieldName, lower, upper, includeLower, includeUpper), occurance);
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
            return this.RangeInternal(fieldName, start, end, includeLower, includeUpper, _occurance);
        }

        protected internal IBooleanOperation RangeInternal(string fieldName, string start, string end, bool includeLower, bool includeUpper, BooleanClause.Occur occurance)
        {
            Query.Add(new TermRangeQuery(fieldName, start, end, includeLower, includeUpper), occurance);

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(query, "query");

            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }
            return this.GroupedAnd(fields.ToArray(), fieldVals.ToArray());
        }

        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, IExamineValue[] fieldVals)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(Query, "fieldVals");

            return this.GroupedAndInternal(fields.ToArray(), fieldVals.ToArray(), _occurance);
        }

        protected internal IBooleanOperation GroupedAndInternal(string[] fields, IExamineValue[] fieldVals, BooleanClause.Occur occurance)
        {

            //if there's only 1 query text we want to build up a string like this:
            //(+field1:query +field2:query +field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            Query.Add(GetMultiFieldQuery(fields, fieldVals, BooleanClause.Occur.MUST), occurance);

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(query, "query");

            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }

            return this.GroupedOr(fields.ToArray(), fieldVals.ToArray());
        }

        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params IExamineValue[] fieldVals)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(Query, "query");

            return this.GroupedOrInternal(fields.ToArray(), fieldVals, _occurance);
        }

        protected internal IBooleanOperation GroupedOrInternal(string[] fields, IExamineValue[] fieldVals, BooleanClause.Occur occurance)
        {
            //if there's only 1 query text we want to build up a string like this:
            //(field1:query field2:query field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            Query.Add(GetMultiFieldQuery(fields, fieldVals, BooleanClause.Occur.SHOULD), occurance);

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(query, "query");

            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }

            return this.GroupedNot(fields.ToArray(), fieldVals.ToArray());
        }

        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(query, "query");

            return this.GroupedNotInternal(fields.ToArray(), query, _occurance);
        }

        protected internal IBooleanOperation GroupedNotInternal(string[] fields, IExamineValue[] fieldVals, BooleanClause.Occur occurance)
        {
            //if there's only 1 query text we want to build up a string like this:
            //(!field1:query !field2:query !field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)
            
            Query.Add(GetMultiFieldQuery(fields, fieldVals, BooleanClause.Occur.MUST_NOT), occurance);

            return new LuceneBooleanOperation(this);
        }

        /// <summary>
        /// Creates our own style 'multi field query' used internal for the grouped operations
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="fieldVals"></param>
        /// <param name="occurance"></param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        protected internal BooleanQuery GetMultiFieldQuery(string[] fields, IExamineValue[] fieldVals, BooleanClause.Occur occurance)
        {
            //if there's only 1 query text we want to build up a string like this:
            //(!field1:query !field2:query !field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            var queryVals = new IExamineValue[fields.Length];
            if (fieldVals.Length == 1)
            {
                for (int i = 0; i < queryVals.Length; i++)
                    queryVals[i] = fieldVals[0];
            }
            else
            {
                queryVals = fieldVals;
            }

            var qry = new BooleanQuery();
            for (int i = 0; i < fields.Length; i++)
            {
                qry.Add(this.GetFieldInternalQuery(fields[i], queryVals[i], true), occurance);
            }

            return qry;
        }

        public IBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params string[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(query, "query");
            Enforcer.ArgumentNotNull(operations, "operations");

            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }

            return this.GroupedFlexible(fields.ToArray(), operations.ToArray(), fieldVals.ToArray());
        }

        public IBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params IExamineValue[] fieldVals)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(Query, "query");
            Enforcer.ArgumentNotNull(operations, "operations");

            return this.GroupedFlexibleInternal(fields.ToArray(), operations.ToArray(), fieldVals, _occurance);
        }

        protected internal IBooleanOperation GroupedFlexibleInternal(string[] fields, BooleanOperation[] operations, IExamineValue[] fieldVals, BooleanClause.Occur occurance)
        {
            //if there's only 1 query text we want to build up a string like this:
            //(field1:query field2:query field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            var flags = new BooleanClause.Occur[operations.Count()];
            for (int i = 0; i < flags.Length; i++)
                flags[i] = operations.ElementAt(i).ToLuceneOccurance();

            var queryVals = new IExamineValue[fields.Length];
            if (fieldVals.Length == 1)
            {
                for (int i = 0; i < queryVals.Length; i++)
                    queryVals[i] = fieldVals[0];
            }
            else
            {
                queryVals = fieldVals;
            }

            var qry = new BooleanQuery();
            for (int i = 0; i < fields.Length; i++)
            {
                qry.Add(this.GetFieldInternalQuery(fields[i], queryVals[i], true), flags[i]);
            }

            this.Query.Add(qry, occurance);

            return new LuceneBooleanOperation(this);
        }

        /// <summary>
        /// Passes a raw search query to the provider to handle
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public ISearchCriteria RawQuery(string query)
        {
            this.Query.Add(this.QueryParser.Parse(query), this._occurance);            
            return this;
        }

        /// <summary>
        /// Orders the results by the specified fields
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public IBooleanOperation OrderBy(params string[] fieldNames)
        {
            Enforcer.ArgumentNotNull(fieldNames, "fieldNames");

            return this.OrderByInternal(false, fieldNames);
        }

        /// <summary>
        /// Orders the results by the specified fields in a descending order
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
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
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        protected internal IBooleanOperation OrderByInternal(bool descending, params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                this.SortFields.Add(new SortField(LuceneIndexer.SortedFieldNamePrefix + fieldName, SortField.STRING, descending));
            }

            return new LuceneBooleanOperation(this);
        }

        #endregion
    }
}
