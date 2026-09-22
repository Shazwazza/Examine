using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Xml;

namespace Examine
{
    public static class ObjectExtensions
    {
        // Caches the reflection-derived property list per type since TypeDescriptor.GetProperties(object)
        // performs the same reflection/attribute lookup work on every call for a given type.
        // Only used for types whose property set is fixed for the CLR type (i.e. not backed by an
        // instance-specific ICustomTypeDescriptor/TypeDescriptionProvider), and invalidated whenever
        // TypeDescriptor.Refresh(...) is called for a cached type so stale metadata isn't retained.
        private static readonly ConcurrentDictionary<Type, PropertyDescriptorCollection> s_propertyCache = new();

        static ObjectExtensions() => TypeDescriptor.Refreshed += e => s_propertyCache.TryRemove(e.TypeChanged, out _);

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
                    throw new InvalidOperationException($"The input object is already of type {typeof(IDictionary)}");

                // Instance-aware descriptors (e.g. ICustomTypeDescriptor implementations, or objects
                // registered with a per-instance TypeDescriptionProvider via TypeDescriptor.AddProvider)
                // can expose a property set that differs per-instance, so they must always be resolved
                // per-object rather than cached per-Type.
                var type = o.GetType();
                PropertyDescriptorCollection props = o is ICustomTypeDescriptor || TypeDescriptor.GetProvider(o) != TypeDescriptor.GetProvider(type)
                    ? TypeDescriptor.GetProperties(o)
                    : s_propertyCache.GetOrAdd(type, t => TypeDescriptor.GetProperties(t));
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
