using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UmbracoExamine.Config;

namespace Examine.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class SearchTest
    {
        public SearchTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Initialize and Cleanup
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            var d = IndexInit.CreateFromTemplate();
            IndexInit.UpdateIndexPaths();
        }

        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            IndexInit.RemoveWorkingIndex();
        } 
        #endregion

        #region Additional test attributes
       
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestSimpleSearch()
        {
            var result = ExamineManager.Instance.Search("home", 10, false);
            Assert.IsTrue(result.Count() > 0, "Results returned for 'home' search: " + result.Count());
        }
    }
}
