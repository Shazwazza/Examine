using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Examine.LuceneEngine.Providers;

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
        public SerializableDictionary() : base()
        {

        }
        protected SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;

            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();
                reader.ReadStartElement("value");

                //if it's string, then get the CData
                TValue value;
                if (typeof(TValue).Equals(typeof(string)))
                {
                    value = (TValue)reader.ReadContentAsObject();
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

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));
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
