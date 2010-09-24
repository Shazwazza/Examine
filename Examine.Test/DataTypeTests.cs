using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Examine.Test
{
    [TestClass]
    public class DataTypeTests
    {
        /// <summary>
        /// Test range query with a DateTime structure
        /// </summary>
        [TestMethod]
        public void FluentApiTests_Date_Range_SimpleIndexSet()
        {
            ////Arrange
            //rebuild now            
            var now = DateTime.Now;
            Thread.Sleep(1000);
            m_Indexer.RebuildIndex();
            Thread.Sleep(5000);

            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("DateCreated", now, DateTime.Now).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("DateCreated", now.AddDays(-1), now.AddSeconds(-1)).Compile();

            ////Act
            var results = m_Searcher.Search(filter);
            var resultsNotFound = m_Searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Date.Year structure
        /// </summary>
        [TestMethod]
        public void FluentApiTests_Date_Range_Year_SimpleIndexSet()
        {
            ////Arrange
            //rebuild now            
            var now = DateTime.Now;
            Thread.Sleep(1000);
            m_Indexer.RebuildIndex();
            Thread.Sleep(5000);

            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("YearCreated", now.Year, DateTime.Now.Year).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("YearCreated", 2008, 2009).Compile();

            ////Act
            var results = m_Searcher.Search(filter);
            var resultsNotFound = m_Searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Date.Month structure
        /// </summary>
        [TestMethod]
        public void FluentApiTests_Date_Range_Month_SimpleIndexSet()
        {
            ////Arrange
            //rebuild now            
            var now = DateTime.Now;
            Thread.Sleep(1000);
            m_Indexer.RebuildIndex();
            Thread.Sleep(5000);

            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("MonthCreated", now.Month, DateTime.Now.Month).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("MonthCreated", now.Month - 2, now.Month - 1).Compile();

            ////Act
            var results = m_Searcher.Search(filter);
            var resultsNotFound = m_Searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Date.Day structure
        /// </summary>
        [TestMethod]
        public void FluentApiTests_Date_Range_Day_SimpleIndexSet()
        {
            ////Arrange
            //rebuild now            
            var now = DateTime.Now;
            Thread.Sleep(1000);
            m_Indexer.RebuildIndex();
            Thread.Sleep(5000);

            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("DayCreated", now.Day, DateTime.Now.Day).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("DayCreated", now.Day - 2, now.Day - 1).Compile();

            ////Act
            var results = m_Searcher.Search(filter);
            var resultsNotFound = m_Searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Date.Hour structure
        /// </summary>
        [TestMethod]
        public void FluentApiTests_Date_Range_Hour_SimpleIndexSet()
        {
            ////Arrange
            //rebuild now            
            var now = DateTime.Now;
            Thread.Sleep(1000);
            m_Indexer.RebuildIndex();
            Thread.Sleep(5000);

            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("HourCreated", now.Hour, DateTime.Now.Hour).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("HourCreated", now.Hour - 2, now.Hour - 1).Compile();

            ////Act
            var results = m_Searcher.Search(filter);
            var resultsNotFound = m_Searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Date.Minute structure
        /// </summary>
        [TestMethod]
        public void FluentApiTests_Date_Range_Minute_SimpleIndexSet()
        {
            ////Arrange
            //rebuild now            
            var now = DateTime.Now;
            Thread.Sleep(1000);
            m_Indexer.RebuildIndex();
            Thread.Sleep(5000);

            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("MinuteCreated", now.Hour, DateTime.Now.Hour).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("MinuteCreated", now.Hour - 2, now.Hour - 1).Compile();

            ////Act
            var results = m_Searcher.Search(filter);
            var resultsNotFound = m_Searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }


        /// <summary>
        /// test range query with a Number structure
        /// </summary>
        [TestMethod]
        public void FluentApiTests_Number_Range_SimpleIndexSet()
        {

            ////Arrange
            //rebuild now            
            //m_Indexer.RebuildIndex();

            //all numbers should be between 0 and 100 based on the data source
            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("SomeNumber", 0, 100).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("SomeNumber", 101, 200).Compile();

            ////Act
            var results = m_Searcher.Search(filter);
            var resultsNotFound = m_Searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Float structure
        /// </summary>
        [TestMethod]
        public void FluentApiTests_Float_Range_SimpleIndexSet()
        {
            ////Arrange            
            //rebuild now            
            //m_Indexer.RebuildIndex();

            //all numbers should be between 0 and 100 based on the data source
            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("SomeFloat", 0, 100).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("SomeFloat", 101, 200).Compile();

            ////Act
            var results = m_Searcher.Search(filter);
            var resultsNotFound = m_Searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Double structure
        /// </summary>
        [TestMethod]
        public void FluentApiTests_Double_Range_SimpleIndexSet()
        {
            ////Arrange            
            //rebuild now            
            //m_Indexer.RebuildIndex();

            //all numbers should be between 0 and 100 based on the data source
            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("SomeDouble", 0, 100).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("SomeDouble", 101, 200).Compile();

            ////Act
            var results = m_Searcher.Search(filter);
            var resultsNotFound = m_Searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Double structure
        /// </summary>
        [TestMethod]
        public void FluentApiTests_Long_Range_SimpleIndexSet()
        {
            ////Arrange            
            //rebuild now            
            //m_Indexer.RebuildIndex();

            //all numbers should be between 0 and 100 based on the data source
            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("SomeLong", 0, 100).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("SomeLong", 101, 200).Compile();

            ////Act
            var results = m_Searcher.Search(filter);
            var resultsNotFound = m_Searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        private static IndexInitializer m_Init;
        private static ISearcher m_Searcher;
        private static IIndexer m_Indexer;

        [TestInitialize()]
        public void Initialize()
        {
            m_Init = new IndexInitializer();

            m_Searcher = ExamineManager.Instance.SearchProviderCollection["SimpleSearcher"];
            m_Indexer = ExamineManager.Instance.IndexProviderCollection["SimpleIndexer"];

            //ensure everything is rebuilt before testing
            m_Indexer.RebuildIndex();
        }
    }
}
