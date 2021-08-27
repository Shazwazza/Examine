using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Examine.Lucene;
using Examine.Lucene.Analyzers;
using Examine.Lucene.Indexing;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Pattern;
using Lucene.Net.Analysis.Standard;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Examine.Test.Examine.Lucene.Analyzers
{

    [TestFixture]
    public class PatternAnalyzerTests : ExamineBaseTest
    {
        [Test]
        public void Tokenizes()
        {
            var pattern = new Regex(@"^(\d{3} \d{3} \d{4})$", RegexOptions.Compiled);

            TokenStream stream = new PatternTokenizer(
                new StringReader("403 222 1234"),
                pattern,
                1);
            string @out = stream.GetString();

            Assert.AreEqual("403 222 1234", @out);
            
            stream = new PatternTokenizer(
                new StringReader("1234 123 1234"),
                pattern,
                1);
            @out = stream.GetString();

            Assert.AreEqual(string.Empty, @out);
        }

        [Test]
        public void Phone_Number()
        {

            var valueTypes = new Dictionary<string, IFieldValueTypeFactory>
            {
                ["phone"] = new DelegateFieldValueTypeFactory(name =>
                                new GenericAnalyzerFieldValueType(
                                    name,
                                    NullLoggerFactory.Instance,
                                    new PatternAnalyzer(@"^(\d{3}\s\d{3}\s\d{4})$", 1)))
            };

            var analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
            using (var luceneDir = new RandomIdRAMDirectory())
            using (var indexer = GetTestIndex(
                luceneDir,
                analyzer,
                new FieldDefinitionCollection(new FieldDefinition("phone", "phone")),
                indexValueTypesFactory: valueTypes))
            {
                indexer.IndexItems(new[] {
                    ValueSet.FromObject(1.ToString(), "content",
                        new { phone = "1234 123 1234", bodyText = "Zanzibar is in Africa"}),
                    ValueSet.FromObject(2.ToString(), "content",
                        new { phone = "403 222 1234", bodyText = "In Canada there is a town called Sydney in Nova Scotia"}),
                    ValueSet.FromObject(3.ToString(), "content",
                        new { phone = "", bodyText = "Sydney is the capital of NSW in Australia"})
                    });

                var reader = indexer.IndexWriter.IndexWriter.GetReader(false);
                var doc1 = reader.Document(0);
                var phoneField = doc1.GetField("phone");
                Console.Write(phoneField);

                var searcher = indexer.Searcher;

                var query = searcher.CreateQuery("content")
                    .Field("phone", "403 222 1234");

                Console.Write(query);

                var results = query.Execute();

                var results2 = searcher.CreateQuery("content")
                    .Field("phone", "1234 123 1234")
                    .Execute();

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(1, results.TotalItemCount, "didn't find valid phone number");
                    Assert.AreEqual(0, results2.TotalItemCount, "found invalid phone numbers");
                });


            }
        }
    }
}
