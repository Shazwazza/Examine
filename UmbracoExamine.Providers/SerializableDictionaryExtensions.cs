using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace UmbracoExamine.Providers
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
                sw.Close();
            }
            using (var fileWriter = fi.CreateText())
            {
                fileWriter.Write(output);
                fileWriter.Close();
            }
        }

        public static void SaveToDisk<TKey, TValue>(this Dictionary<TKey, TValue> d, FileInfo fi)
        {
            d.ToSerializableDictionary().SaveToDisk(fi);
        }

        public static void ReadFromDisk<TKey, TValue>(this SerializableDictionary<TKey, TValue> sd, FileInfo fi)
        {
            //get the dictionary object from the file data
            XmlSerializer xs = new XmlSerializer(typeof(SerializableDictionary<string, string>));
            var deserialized = new SerializableDictionary<TKey, TValue>();
            using (var s = fi.OpenText())
            {
                deserialized = xs.Deserialize(s) as SerializableDictionary<TKey, TValue>;
                s.Close();
            }
            sd.Clear();
            foreach (var x in deserialized)
            {
                sd.Add(x.Key, x.Value);
            }
        }
    }
}
