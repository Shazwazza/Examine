using Examine.Lucene.Search;
using Lucene.Net.Documents;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene.Search
{
    [TestFixture]
    public class LuceneSearchExecutorTests
    {
        [Test]
        public void Given_StoredBinaryField_When_CreatingSearchResult_Then_AllValuesContainsFieldWithEmptyValues()
        {
            var doc = new Document
            {
                new StringField("id", "1", Field.Store.YES),
                new StringField("nodeName", "hello", Field.Store.YES),
                new StoredField("blob", new byte[] { 1, 2, 3 })
            };

            var result = LuceneSearchExecutor.CreateSearchResult(doc, 1.0f, 0);

            Assert.IsTrue(result.AllValues.ContainsKey("blob"));
            Assert.AreEqual(0, result.AllValues["blob"].Count);
            Assert.AreEqual("hello", result.Values["nodeName"]);
        }
    }
}
