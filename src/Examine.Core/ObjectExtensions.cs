using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Xml;

namespace Examine
{
    /// <summary>
    /// Extensions for objects
    /// </summary>
    public static class ObjectExtensions
    {
        // Caches TypeDescriptor.GetProperties(Type) results per Type to avoid repeated
        // reflection/property-descriptor discovery when the same POCO type is converted
        // repeatedly (e.g. bulk indexing many documents of the same shape).
        private static readonly ConcurrentDictionary<Type, PropertyDescriptorCollection> s_propertiesCache = new();

        /// <summary>
        /// Turns object into dictionary
        /// </summary>
        /// <param name="o"></param>
        /// <param name="ignoreProperties">Properties to ignore</param>
        /// <returns></returns>
        public static IDictionary<string, object> ConvertObjectToDictionary(object o, params string[] ignoreProperties)
        {
            if (o != null)
            {
                if (o is IDictionary)
                {
                    throw new InvalidOperationException($"The input object is already of type {typeof(IDictionary)}");
                }

                var props = s_propertiesCache.GetOrAdd(o.GetType(), static t => TypeDescriptor.GetProperties(t));
                var ignoreSet = ignoreProperties.Length == 0 ? null : new HashSet<string>(ignoreProperties);
                var d = new Dictionary<string, object>();
                foreach (PropertyDescriptor prop in props)
                {
                    if (ignoreSet != null && ignoreSet.Contains(prop.Name))
                    {
                        continue;
                    }

                    var val = prop.GetValue(o);
                    if (val != null)
                    {
                        d.Add(prop.Name, val);
                    }
                }
                return d;
            }
            return new Dictionary<string, object>();
        }
    }
}
