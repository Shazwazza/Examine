using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Examine.Test
{
    [TestClass]
    public class IndexTest
    {
        #region Initialize and Cleanup

        private static IndexInit m_Init = new IndexInit("IndexWorkingTest");

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            m_Init.RemoveWorkingIndex();

            var d = m_Init.CreateFromTemplate();
            m_Init.UpdateIndexPaths();
        }

        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            //IndexInit.RemoveWorkingIndex();
        }

        #endregion

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
