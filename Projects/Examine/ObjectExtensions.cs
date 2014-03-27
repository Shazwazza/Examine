using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Xml;

namespace Examine
{
    internal static class ObjectExtensions
    {       
        /// <summary>
        /// Turns object into dictionary
        /// </summary>
        /// <param name="o"></param>
        /// <param name="ignoreProperties">Properties to ignore</param>
        /// <returns></returns>
        internal static IDictionary<string, TVal> ToDictionary<TVal>(this object o, params string[] ignoreProperties)
        {
            if (o != null)
            {
                var props = TypeDescriptor.GetProperties(o);
                var d = new Dictionary<string, TVal>();
                foreach (var prop in props.Cast<PropertyDescriptor>().Where(x => !ignoreProperties.Contains(x.Name)))
                {
                    var val = prop.GetValue(o);
                    if (val != null)
                    {
                        d.Add(prop.Name, (TVal)val);
                    }
                }
                return d;
            }
            return new Dictionary<string, TVal>();
        }
    }
}
