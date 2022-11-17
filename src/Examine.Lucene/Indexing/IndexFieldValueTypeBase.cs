using System;
using System.ComponentModel;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene.Indexing
{
    /// <inheritdoc/>
    public abstract class IndexFieldValueTypeBase : IIndexFieldValueType
    {
        /// <inheritdoc/>
        public string FieldName { get; }

        /// <inheritdoc/>
        public virtual string SortableFieldName => null;

        /// <inheritdoc/>
        public bool Store { get; }

        /// <inheritdoc/>
        protected IndexFieldValueTypeBase(string fieldName, ILoggerFactory loggerFactory, bool store = true)
        {
            FieldName = fieldName;
            Logger = loggerFactory.CreateLogger(GetType());
            Store = store;
        }

        /// <inheritdoc/>
        public virtual Analyzer Analyzer => null;

        /// <summary>
        /// The logger
        /// </summary>
        public ILogger Logger { get; }

        /// <inheritdoc/>
        public virtual void AddValue(Document doc, object value) => AddSingleValueInternal(doc, value);

        private void AddSingleValueInternal(Document doc, object value)
        {
            if (value != null)
            {
                AddSingleValue(doc, value);
            }
        }

        /// <summary>
        /// Adds a single value to the document
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="value"></param>
        protected abstract void AddSingleValue(Document doc, object value);

        /// <summary>
        /// By default returns a <see cref="TermQuery"/>
        /// </summary>
        /// <param name="query"></param>
        /// 
        /// <returns></returns>
        public virtual Query GetQuery(string query) => new TermQuery(new Term(FieldName, query));


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
                    if (Logger.IsEnabled(LogLevel.Debug))
                    {
                        Logger.LogDebug(ex, "An conversion error occurred with from inputConverter.ConvertTo {FromValue} to {ToValueType}", val, typeof(T));
                    }
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
                    if (Logger.IsEnabled(LogLevel.Debug))
                    {
                        Logger.LogDebug(ex, "An conversion error occurred with outputConverter.ConvertFrom from {FromValue} to {ToValueType}", val, typeof(T));
                    }
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
                if (Logger.IsEnabled(LogLevel.Debug))
                {
                    Logger.LogDebug(ex, "An conversion error occurred with Convert.ChangeType from {FromValue} to {ToValueType}", val, typeof(T));
                }
                parsedVal = default(T);
                return false;
            }
        }

    }
}
