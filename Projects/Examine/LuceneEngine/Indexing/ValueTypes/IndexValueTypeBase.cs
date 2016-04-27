using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing.Filters;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing.ValueTypes
{
    public abstract class IndexValueTypeBase : IIndexValueType
    {
        public string FieldName { get; private set; }
        public bool Store { get; private set; }

        public Func<object, IEnumerable<object>> Splitter { get; set; }


        public IValueFilter ValueFilter { get; set; }
        
        public IndexValueTypeBase SetSeparator(string sep)
        {
            Splitter = value => ("" + value).Split(new [] {sep}, StringSplitOptions.RemoveEmptyEntries);
            return this;
        }

        protected IndexValueTypeBase(string fieldName, bool store = true)
        {
            FieldName = fieldName;
            Store = store;            
        }

        public virtual void SetupAnalyzers(PerFieldAnalyzerWrapper analyzer)
        {
            
        }

        public virtual void AddValue(Document doc, object value)
        {
            if (Splitter != null)
            {
                foreach (var val in Splitter(value))
                {
                    AddSingleValueInternal(doc, val);
                }
            }
            else
            {
                AddSingleValueInternal(doc, value);
            }
        }

        private void AddSingleValueInternal(Document doc, object value)
        {
            if (ValueFilter != null)
            {
                value = ValueFilter.Filter(value);
            }

            if (value != null)
            {
                AddSingleValue(doc, value);
            }
        }

        protected abstract void AddSingleValue(Document doc, object value);        


        public virtual void AnalyzeReader(ReaderData readerData)
        {            
        }

        public virtual Query GetQuery(string query, Searcher searcher, FacetsLoader facetsLoader, IManagedQueryParameters parameters)
        {
            return new TermQuery(new Term(FieldName, query));
        }

        
        //TODO: We shoud convert this to the TryConvertTo in the umb codebase!

        /// <summary>
        /// Tries to parse a type using the Type's type converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="parsedVal"></param>
        /// <returns></returns>        
        protected static bool TryConvert<T>(object val, out T parsedVal)
            where T : struct
        {
            if (val is T)
            {
                parsedVal = (T) val;
                return true;
            }

            var inputConverter = TypeDescriptor.GetConverter(val);
            if (inputConverter.CanConvertTo(typeof(T)))
            {
                try
                {
                    var converted = inputConverter.ConvertTo(val, typeof(T));
                    parsedVal = (T) converted;
                    return true;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("An error occurred in {0}.{1} inputConverter.ConvertTo(val, typeof(T)) : {2}", nameof(IndexValueTypeBase), nameof(TryConvert), ex);

                    parsedVal = default(T);
                    return false;
                }
            }

            var outputConverter = TypeDescriptor.GetConverter(typeof(T));
            if (outputConverter.CanConvertFrom(val.GetType()))
            {
                try
                {
                    var converted = outputConverter.ConvertFrom(val);
                    parsedVal = (T)converted;
                    return true;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("An error occurred in {0}.{1} outputConverter.ConvertFrom(val) : {2}", nameof(IndexValueTypeBase), nameof(TryConvert), ex);

                    parsedVal = default(T);
                    return false;
                }
            }

            try
            {
                var casted = Convert.ChangeType(val, typeof(T));
                parsedVal = (T)casted;
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("An error occurred in {0}.{1} Convert.ChangeType(val, typeof(T)) : {2}", nameof(IndexValueTypeBase), nameof(TryConvert), ex);

                parsedVal = default(T);
                return false;
            }

            parsedVal = default(T);
            return false;
        }


        public virtual IHighlighter GetHighlighter(Query query, Searcher searcher, FacetsLoader facetsLoader)
        {
            return null;
        }

        public virtual IFacetExtractor CreateFacetExtractor()
        {
            return null;
        }
    }
}
