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
    }
}