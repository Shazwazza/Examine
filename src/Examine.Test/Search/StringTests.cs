
using NUnit.Framework;

namespace Examine.Test.Search
{
    [TestFixture]
	public class StringTests //: AbstractPartialTrustFixture<StringTests>
    {
        [Test]
        public void Search_Remove_Stop_Words()
        {

            var stringPhrase1 = "hello my name is \"Shannon Deminick\" \"and I like to code\", here is a stop word and or two";
            var stringPhrase2 = "\"into the darkness\" this is a sentence with a quote at \"the front and the end\"";

            var parsed1 = stringPhrase1.RemoveStopWords();
            var parsed2 = stringPhrase2.RemoveStopWords();

            Assert.AreEqual("hello my name \"Shannon Deminick\" \"and I like to code\" , here stop word two", parsed1);
            Assert.AreEqual("\"into the darkness\" sentence quote \"the front and the end\"", parsed2);
        }

        [Test]
        public void Search_Remove_Stop_Words_Uneven_Quotes()
        {

            var stringPhrase1 = "hello my name is \"Shannon Deminick \"and I like to code\", here is a stop word and or two";

            var parsed1 = stringPhrase1.RemoveStopWords();

            Assert.AreEqual("hello my name \"Shannon Deminick\" I like code , here stop word two", parsed1);

        }
        
    }
}