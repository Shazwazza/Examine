using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Examine.Test.Search
{
    [TestClass]
    public class DataTypeTests
    {
        /// <summary>
        /// Test range query with a DateTime structure
        /// </summary>
        [TestMethod]
        public void DataTypesTests_Date_Range_SimpleIndexSet()
        {
            ////Arrange

            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("DateCreated", m_ReIndexDateTime, DateTime.Now, true, true).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("DateCreated", m_ReIndexDateTime.AddDays(-1), m_ReIndexDateTime.AddSeconds(-1), true, true).Compile();

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
        public void DataTypesTests_Date_Range_Year_SimpleIndexSet()
        {
            ////Arrange

            var criteria = m_Searcher.CreateSearchCriteria();
            
            //Shouldn't there be results for searching the current year? 2010 -> 2010 ?
            var filter = criteria.Range("YearCreated", m_ReIndexDateTime, DateTime.Now, true, true, SearchCriteria.DateResolution.Year).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("YearCreated", DateTime.Now.AddYears(-2), DateTime.Now.AddYears(-1), true, true).Compile();

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
        public void DataTypesTests_Date_Range_Month_SimpleIndexSet()
        {
            ////Arrange

            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("MonthCreated", m_ReIndexDateTime, DateTime.Now, true, true, SearchCriteria.DateResolution.Month).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("MonthCreated", m_ReIndexDateTime.AddMonths(-2), m_ReIndexDateTime.AddMonths(-1), true, true).Compile();

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
        public void DataTypesTests_Date_Range_Day_SimpleIndexSet()
        {
            ////Arrange

            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("DayCreated", m_ReIndexDateTime, DateTime.Now, true, true, SearchCriteria.DateResolution.Day).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("DayCreated", m_ReIndexDateTime.AddDays(-2), m_ReIndexDateTime.AddDays(-1), true, true).Compile();

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
        public void DataTypesTests_Date_Range_Hour_SimpleIndexSet()
        {
            ////Arrange

            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("HourCreated", m_ReIndexDateTime, DateTime.Now, true, true, SearchCriteria.DateResolution.Hour).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("HourCreated", m_ReIndexDateTime.AddHours(-2), m_ReIndexDateTime.AddHours(-1), true, true).Compile();

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
        public void DataTypesTests_Date_Range_Minute_SimpleIndexSet()
        {
            ////Arrange

            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("MinuteCreated", m_ReIndexDateTime, DateTime.Now, true, true, SearchCriteria.DateResolution.Minute).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("MinuteCreated", m_ReIndexDateTime.AddMinutes(-20), m_ReIndexDateTime.AddMinutes(-1), true, true).Compile();

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
        public void DataTypesTests_Number_Range_SimpleIndexSet()
        {

            ////Arrange
            //all numbers should be between 0 and 100 based on the data source
            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("SomeNumber", 0, 100, true, true).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("SomeNumber", 101, 200, true, true).Compile();

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
        public void DataTypesTests_Float_Range_SimpleIndexSet()
        {
            ////Arrange            

            //all numbers should be between 0 and 100 based on the data source
            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("SomeFloat", 0f, 100f, true, true).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("SomeFloat", 101f, 200f, true, true).Compile();

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
        public void DataTypesTests_Double_Range_SimpleIndexSet()
        {
            ////Arrange            

            //all numbers should be between 0 and 100 based on the data source
            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("SomeDouble", 0d, 100d, true, true).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("SomeDouble", 101d, 200d, true, true).Compile();

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
        public void DataTypesTests_Long_Range_SimpleIndexSet()
        {
            ////Arrange            

            //all numbers should be between 0 and 100 based on the data source
            var criteria = m_Searcher.CreateSearchCriteria();
            var filter = criteria.Range("SomeLong", 0L, 100L, true, true).Compile();

            var criteriaNotFound = m_Searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("SomeLong", 101L, 200L, true, true).Compile();

            ////Act
            var results = m_Searcher.Search(filter);
            var resultsNotFound = m_Searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }
        
        private static ISearcher m_Searcher;
        private static IIndexer m_Indexer;
        private static DateTime m_ReIndexDateTime;

        [TestInitialize()]
        public void Initialize()
        {
            IndexInitializer.Initialize();

            m_Searcher = ExamineManager.Instance.SearchProviderCollection["SimpleSearcher"];
            m_Indexer = ExamineManager.Instance.IndexProviderCollection["SimpleIndexer"];

            //ensure everything is rebuilt before testing
            m_ReIndexDateTime = DateTime.Now;
            Thread.Sleep(1000);
            m_Indexer.RebuildIndex();
            Thread.Sleep(1000);
        }
    }
}
