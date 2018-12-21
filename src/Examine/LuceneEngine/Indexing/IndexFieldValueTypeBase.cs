using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing
{
    public abstract class IndexFieldValueTypeBase : IIndexFieldValueType
    {
        public string FieldName { get; }

        //by default it will not be sortable
        public virtual string SortableFieldName => null;

        public bool Store { get; }
        
        protected IndexFieldValueTypeBase(string fieldName, bool store = true)
        {
            FieldName = fieldName;
            Store = store;            
        }

        public virtual void SetupAnalyzers(PerFieldAnalyzerWrapper analyzer)
        {
            
        }

        public virtual void AddValue(Document doc, object value)
        {
            AddSingleValueInternal(doc, value);
        }

        private void AddSingleValueInternal(Document doc, object value)
        {
            if (value != null)
            {
                AddSingleValue(doc, value);
            }
        }

        protected abstract void AddSingleValue(Document doc, object value);        
        
        /// <summary>
        /// By default returns a <see cref="TermQuery"/>
        /// </summary>
        /// <param name="query"></param>
        /// <param name="searcher"></param>
        /// <returns></returns>
        public virtual Query GetQuery(string query, Searcher searcher)
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
        {
            if (val == null)
            {
                parsedVal = default(T);
                return false;
            }

            if (val is T)
            {
                parsedVal = (T) val;
                return true;
            }

            if (typeof(T) == typeof(string))
            {
                parsedVal = (T)(object)val.ToString();
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
                    Trace.TraceError("An error occurred in {0}.{1} inputConverter.ConvertTo(val, typeof(T)) : {2}", nameof(IndexFieldValueTypeBase), nameof(TryConvert), ex);

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
                    Trace.TraceError("An error occurred in {0}.{1} outputConverter.ConvertFrom(val) : {2}", nameof(IndexFieldValueTypeBase), nameof(TryConvert), ex);

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
                Trace.TraceError("An error occurred in {0}.{1} Convert.ChangeType(val, typeof(T)) : {2}", nameof(IndexFieldValueTypeBase), nameof(TryConvert), ex);

                parsedVal = default(T);
                return false;
            }
        }

    }
}
