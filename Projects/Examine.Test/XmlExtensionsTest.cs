using System.Xml.Linq;
using System.Xml.XPath;
using Examine.LuceneEngine;

using NUnit.Framework;

namespace Examine.Test
{
    [TestFixture]
    public class XmlExtensionsTest 
    {
        [Test]
        public void ToXDocument_With_Root_Node_Check()
        {
            var xml = "<root><blah></blah></root>";
            var iterator = XDocument.Parse(xml).CreateNavigator().Select("/");
            var result = iterator.ToXDocument();
            Assert.AreEqual(null, result);
        }
        [Test]
        public void Select_Examine_Value_Normal()
        {
            var xml = "<someNode id='1234'><blah>Hello world</blah><xmlVal><some><xml><structure></structure></xml></some></xmlVal></someNode>";

            var xNode = XElement.Parse(xml);

            var result = xNode.SelectExamineDataValue("blah");

            Assert.AreEqual("Hello world", result);
        }

        [Test]
        public void Select_Examine_Value_Xml_Fragment()
        {
            var xml = "<someNode id='1234'><blah>Hello world</blah><xmlVal><some><xml><structure></structure></xml></some></xmlVal></someNode>";

            var xNode = XElement.Parse(xml);

            var result = xNode.SelectExamineDataValue("xmlVal");

            Assert.AreEqual("<some><xml><structure></structure></xml></some>", result);
        }

    }
}