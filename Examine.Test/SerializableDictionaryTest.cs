using System.IO;
using Examine.LuceneEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;
using System.Text;

namespace Examine.Test
{
    
    
    /// <summary>
    ///This is a test class for SerializableDictionaryTest and is intended
    ///to contain all SerializableDictionaryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SerializableDictionaryTest
    {


        
        [TestMethod]
        public void SerializableDictionaryTest_Save_To_Disk_Read_From_Disk()
        {
            //arrange

            var target = new SerializableDictionary<string, string>()
                             {
                                 { "Name", "Shannon Deminick" },
                                 { "Email", "sdeminick@gmail.com" }
                             };

            //act
            var file = new FileInfo(Path.Combine(Environment.CurrentDirectory, "SerializedDictionary.txt"));
            target.SaveToDisk(file);

            //assert
            file.Refresh();
            Assert.IsTrue(file.Exists);
            var result = new SerializableDictionary<string, string>();
            result.ReadFromDisk(file);
            Assert.AreEqual(target["Name"], result["Name"]);
            Assert.AreEqual(target["Email"], result["Email"]);
        }

        [TestMethod]
        public void SerializableDictionaryTest_Invalid_Encoding()
        {
            //arrange

            var target = new SerializableDictionary<string, string>()
                             {
                                 { "Name", "☃ ☁ ☠" },
                                 { "Email", "☢ ☥ ☺" }
                             };

            SerializableDictionaryExtensions.DefaultFileEncoding = Encoding.ASCII;

            //act
            var file = new FileInfo(Path.Combine(Environment.CurrentDirectory, "SerializedDictionary.txt"));
            target.SaveToDisk(file);

            //assert
            file.Refresh();
            Assert.IsTrue(file.Exists);
            var result = new SerializableDictionary<string, string>();
            result.ReadFromDisk(file);
            
            //reset it
            SerializableDictionaryExtensions.DefaultFileEncoding = Encoding.UTF8;
            
            //with the wrong encoding this will fail!
            Assert.AreNotEqual(target["Name"], result["Name"]);
            Assert.AreNotEqual(target["Email"], result["Email"]);
        }

      
    }
}
