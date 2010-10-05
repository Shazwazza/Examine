using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Examine.LuceneEngine
{
    public static class SerializableDictionaryExtensions
    {

        public static SerializableDictionary<TKey, TValue> ToSerializableDictionary<TKey, TValue>(this Dictionary<TKey, TValue> d)
        {
            SerializableDictionary<TKey, TValue> sd = new SerializableDictionary<TKey, TValue>();
            d.ToList().ForEach(x =>
            {
                sd.Add(x.Key, x.Value);
            });
            return sd;
        }

        /// <summary>
        /// Returns a Dictionary from a type with a specified value to a dictionary that has a different type for it's value
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TConvertVal"></typeparam>
        /// <param name="sd"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TConvertVal> ToDictionary<TKey, TValue, TConvertVal>(this SerializableDictionary<TKey, TValue> sd, Converter<TValue, TConvertVal> c)
        {
            Dictionary<TKey, TConvertVal> d = new Dictionary<TKey, TConvertVal>();
            sd.ToList().ForEach(x =>
            {
                d.Add(x.Key, c.Invoke(x.Value));
            });
            return d;
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this SerializableDictionary<TKey, TValue> sd)
        {
            Dictionary<TKey, TValue> d = new Dictionary<TKey, TValue>();
            sd.ToList().ForEach(x =>
            {
                d.Add(x.Key, x.Value);
            });
            return d;
        }

        private static void SaveToDisk<TKey, TValue>(this SerializableDictionary<TKey, TValue> sd, FileInfo fi)
        {
            XmlSerializer xs = new XmlSerializer(sd.GetType());
            string output = "";
            using (StringWriter sw = new StringWriter())
            {
                xs.Serialize(sw, sd);
                output = sw.ToString();
            }
            using (var fileWriter = fi.CreateText())
            {
                fileWriter.Write(output);
            }
        }

        public static void SaveToDisk<TKey, TValue>(this Dictionary<TKey, TValue> d, FileInfo fi)
        {
            d.ToSerializableDictionary().SaveToDisk(fi);
        }

        public static void ReadFromDisk<TKey, TValue>(this SerializableDictionary<TKey, TValue> sd, FileInfo fi)
        {
            //get the dictionary object from the file data
            XmlSerializer xs = new XmlSerializer(typeof(SerializableDictionary<TKey, TValue>));
            var deserialized = new SerializableDictionary<TKey, TValue>();
            using (var s = fi.OpenText())
            {
                deserialized = xs.Deserialize(s) as SerializableDictionary<TKey, TValue>;
            }
            sd.Clear();
            foreach (var x in deserialized)
            {
                sd.Add(x.Key, x.Value);
            }
        }
    }
}
