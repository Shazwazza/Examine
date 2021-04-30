using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    public abstract class IndexFieldValueTypeBase : IIndexFieldValueType
    {
        private readonly ILogger<IndexFieldValueTypeBase> _logger;

        public string FieldName { get; }

        //by default it will not be sortable
        public virtual string SortableFieldName => null;

        public bool Store { get; }
        
        protected IndexFieldValueTypeBase(string fieldName, ILogger<IndexFieldValueTypeBase> logger, bool store = true)
        {
            FieldName = fieldName;
            _logger = logger;
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
        public virtual Query GetQuery(string query, IndexSearcher searcher)
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
        protected bool TryConvert<T>(object val, out T parsedVal)
        {
            // TODO: This throws all the time and then logs! 

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
                    _logger.LogDebug(ex, "An conversion error occurred with from inputConverter.ConvertTo {FromValue} to {ToValueType}", val, typeof(T));

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
                    _logger.LogDebug(ex, "An conversion error occurred with outputConverter.ConvertFrom from {FromValue} to {ToValueType}", val, typeof(T));

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
                _logger.LogDebug(ex, "An conversion error occurred with Convert.ChangeType from {FromValue} to {ToValueType}", val, typeof(T));

                parsedVal = default(T);
                return false;
            }
        }

    }
}
