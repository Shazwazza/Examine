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

        private static IndexInit m_Init;

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            m_Init = new IndexInit();
        }

        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            //IndexInit.RemoveWorkingIndex();
        }

        #endregion

        [TestMethod]
        public void TestRebuildIndex()
        {
            ExamineManager.Instance.RebuildIndex();
        }
    }
}
