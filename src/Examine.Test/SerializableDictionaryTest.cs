using System.IO;
using System.Xml.Linq;
using Examine.LuceneEngine;
using System;
using System.Xml;
using System.Text;
using System.Collections.Generic;

using NUnit.Framework;

namespace Examine.Test
{
    
    
    /// <summary>
    ///This is a test class for SerializableDictionaryTest and is intended
    ///to contain all SerializableDictionaryTest Unit Tests
    ///</summary>
    [TestFixture]
    public class SerializableDictionaryTest //: AbstractPartialTrustFixture<SerializableDictionaryTest>
    {

        [Test]
        public void SerializableDictionaryTest_Save_Buffer_To_Disk_Read_From_Disk()
        {
            //arrange

            var target = new List<Dictionary<string, string>>
                             {
                                 new Dictionary<string, string>
                                     {
                                         { "Name", "Shannon Deminick" },
                                         { "Email", "blah@blah.com" }
                                     },
                                 new Dictionary<string, string>
                                     {
                                         { "Name", "Someone Else" },
                                         { "Email", "person@somewhere.com" }
                                     },
                                new Dictionary<string, string>
                                     {
                                         { "Name", "Hello there" },
                                         { "Email", "hi@you.com" }
                                     },
                             };

            //act
            var file = new FileInfo(Path.Combine(Environment.CurrentDirectory, "BufferedSerializedDictionary.txt"));
            target.SaveToDisk(file);

            //assert
            file.Refresh();
            Assert.IsTrue(file.Exists);
            var result = new List<Dictionary<string, string>>();
            XDocument xml;
            result.ReadFromDisk(file, out xml);

            Assert.AreEqual(target[0]["Name"], result[0]["Name"]);
            Assert.AreEqual(target[0]["Email"], result[0]["Email"]);

            Assert.AreEqual(target[1]["Name"], result[1]["Name"]);
            Assert.AreEqual(target[1]["Email"], result[1]["Email"]);

            Assert.AreEqual(target[2]["Name"], result[2]["Name"]);
            Assert.AreEqual(target[2]["Email"], result[2]["Email"]);
        }
        
        [Test]
        public void SerializableDictionaryTest_Save_To_Disk_Read_From_Disk()
        {
            //arrange

            var target = new Dictionary<string, string>
                             {
                                 { "Name", "Shannon Deminick" },
                                 { "Email", "asdf@blah.com" }
                             };

            //act
            var file = new FileInfo(Path.Combine(Environment.CurrentDirectory, "SerializedDictionary.txt"));
            target.SaveToDisk(file);

            //assert
            file.Refresh();
            Assert.IsTrue(file.Exists);
            var result = new Dictionary<string, string>();
            result.ReadFromDisk(file);
            Assert.AreEqual(target["Name"], result["Name"]);
            Assert.AreEqual(target["Email"], result["Email"]);
        }

        [Test]
        public void SerializableDictionaryTest_Invalid_Encoding()
        {
            //arrange

            var target = new Dictionary<string, string>()
                             {
                                 { "Name", "☃ ☁ ☠" },
                                 { "Email", "☢ ☥ ☺" }
                             };

            DictionaryExtensions.DefaultFileEncoding = Encoding.ASCII;

            //act
            var file = new FileInfo(Path.Combine(Environment.CurrentDirectory, "SerializedDictionary.txt"));
            target.SaveToDisk(file);

            //assert
            file.Refresh();
            Assert.IsTrue(file.Exists);
            var result = new Dictionary<string, string>();
            result.ReadFromDisk(file);
            
            //reset it
            DictionaryExtensions.DefaultFileEncoding = Encoding.UTF8;
            
            //with the wrong encoding this will fail!
            Assert.AreNotEqual(target["Name"], result["Name"]);
            Assert.AreNotEqual(target["Email"], result["Email"]);
        }


	    //public override void TestSetup()
	    //{
	    //}

	    //public override void TestTearDown()
	    //{
	    //}
    }
}
