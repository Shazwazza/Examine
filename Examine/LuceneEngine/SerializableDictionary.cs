using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Examine.LuceneEngine.Providers;
using System.ComponentModel;
using System.Text;

namespace Examine.LuceneEngine
{

    /// <summary>
    /// A Dictionary that is Xml serializable
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks>
    /// Whenever a string is encountered for the value, it is put into a CDATA block.
    /// </remarks>
    [XmlRoot("dictionary")]
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {

        
        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Read the XML
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            var wasEmpty = reader.IsEmptyElement;

            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
                reader.ReadStartElement("key");
                var key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();
                reader.ReadStartElement("value");

                //if it's string, then get the CData
                TValue value;
                if (reader.NodeType == System.Xml.XmlNodeType.CDATA)
                {                   
                    value = (TValue)(object)reader.Value;
                    reader.Read();
                }
                else 
                {
                    value = (TValue)valueSerializer.Deserialize(reader);
                }              
  
                reader.ReadEndElement();
                this.Add(key, value);
                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        /// <summary>
        /// Write the XML
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));
            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement(); // end key
                
                TValue value = this[key];

                //wrap in CData if string
                writer.WriteStartElement("value");
                if (value is string)
                {
                    writer.WriteCData(Convert.ToString(value));
                }
                else
                {                    
                    valueSerializer.Serialize(writer, value);                    
                }
                writer.WriteEndElement(); //end value
                
                writer.WriteEndElement();
            }
        }
        #endregion
    }
}
