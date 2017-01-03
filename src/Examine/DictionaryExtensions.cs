using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Examine
{
    ///<summary>
    /// Extensions for Dictionary objects
    ///</summary>
    public static class DictionaryExtensions
    {
        ///<summary>
        /// The encoding to use when saving the file
        ///</summary>
        internal static Encoding DefaultFileEncoding = Encoding.UTF8;

        /// <summary>
        /// Remove illegal XML characters from a string.
        /// </summary>
        private static string SanitizeXmlString(string xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            var buffer = new StringBuilder(xml.Length);

            foreach (var c in xml.Where(c => IsLegalXmlChar(c)))
            {
                buffer.Append(c);
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Whether a given character is allowed by XML 1.0.
        /// </summary>
        private static bool IsLegalXmlChar(int character)
        {
            return
            (
                 character == 0x9 /* == '\t' == 9   */          ||
                 character == 0xA /* == '\n' == 10  */          ||
                 character == 0xD /* == '\r' == 13  */          ||
                (character >= 0x20 && character <= 0xD7FF) ||
                (character >= 0xE000 && character <= 0xFFFD) ||
                (character >= 0x10000 && character <= 0x10FFFF)
            );
        }

        private static void CleanValues<TKey>(this Dictionary<TKey, string> items)
        {
            foreach(var i in items.Keys.ToArray())
            {
                items[i] = SanitizeXmlString(items[i]);
            }
        }

        public static void SaveToDisk(this IEnumerable<Dictionary<string, string>> items, FileInfo fi)
        {
            var xml = new XDocument();
            var root = new XElement("documents");
            xml.Add(root);
            foreach (var i in items)
            {
                CleanValues(i);

                var docRoot = new XElement("document");
                root.Add(docRoot);
                foreach(var d in i)
                {
                    docRoot.AddKeyValueElement(d);    
                }
            }
            xml.SaveToDisk(fi);
        }

        public static void SaveToDisk<TKey>(this Dictionary<TKey, string> d, FileInfo fi)
        {
            CleanValues(d);

            var xml = new XDocument();
            var root = new XElement("document");
            xml.Add(root);
            foreach(var i in d)
            {
                root.AddKeyValueElement(i);
            }
            xml.SaveToDisk(fi);
        }

        private static void SaveToDisk(this XContainer xml, FileSystemInfo fi)
        {
            //write file using UTF8
            if (fi.Exists)
            {
                //truncate the bytes and re-write
                using (var w = new StreamWriter(
					new FileStream(fi.FullName, FileMode.Truncate), DefaultFileEncoding))
                {
                    w.Write(xml);
                }
            }
            else
            {
                //create a new file (if for some reason its already there it is overwritten)
                using (var w = new StreamWriter(new FileStream(fi.FullName, FileMode.Create), DefaultFileEncoding))
                {
                    w.Write(xml);
                }
            }
        }

        private static void AddKeyValueElement<TKey>(this XContainer xml, KeyValuePair<TKey, string> keyval)
        {
            xml.Add(new XElement("entry",
                                     new XAttribute("key", keyval.Key.ToString()),
                                     new XCData(keyval.Value)));
        }

        public static void ReadFromDisk(this List<Dictionary<string, string>> d, FileInfo fi, out XDocument xDoc)
        {
            d.Clear();
            using (var r = new StreamReader(new FileStream(fi.FullName, FileMode.Open), DefaultFileEncoding))
            {
                xDoc = XDocument.Load(r);
                if (xDoc.Root != null)
                {
                    //loop through the 'documents/document'
                    d.AddRange(
                        xDoc.Root.Elements()
                            .Select(e => e.Elements("entry")
                                                .ToDictionary(x => (string) x.Attribute("key"), x => x.Nodes().OfType<XCData>().Single().Value)));
                }

            }
        }

        public static void ReadFromDisk(this Dictionary<string, string> d, FileInfo fi)
        {
            d.Clear();
            using (var r = new StreamReader(new FileStream(fi.FullName, FileMode.Open), DefaultFileEncoding))
            {
                var xDoc = XDocument.Load(r);
                if (xDoc.Root != null)
                {
                    foreach (var e in xDoc.Root.Elements())
                    {
                        d.Add((string)e.Attribute("key"), e.Nodes().OfType<XCData>().Single().Value);
                    }
                }
                        
            }
        }
    }
}
