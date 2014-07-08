using System;
using System.Diagnostics;
using System.IO;
using Examine.LuceneEngine.Providers;
using Examine.Test.DataServices;

using Lucene.Net.Analysis.Standard;
using System.Threading;
using Lucene.Net.Store;
using NUnit.Framework;

namespace Examine.Test.Search
{
    [TestFixture]
	public class DataTypeTests
    {
        /// <summary>
        /// Test range query with a DateTime structure
        /// </summary>
        [Test]
        public void DataTypesTests_Date_Range_SimpleIndexSet()
        {
            ////Arrange

            var criteria = _searcher.CreateSearchCriteria();
            var filter = criteria.Range("DateCreated", _reIndexDateTime, DateTime.Now, true, true).Compile();

            var criteriaNotFound = _searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("DateCreated", _reIndexDateTime.AddDays(-1), _reIndexDateTime.AddSeconds(-1), true, true).Compile();

            ////Act
            var results = _searcher.Search(filter);
            var resultsNotFound = _searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Date.Year structure
        /// </summary>
        [Test]
        public void DataTypesTests_Date_Range_Year_SimpleIndexSet()
        {
            ////Arrange

            var criteria = _searcher.CreateSearchCriteria();

            //Shouldn't there be results for searching the current year? 2010 -> 2010 ?
            var filter = criteria.Range("YearCreated", _reIndexDateTime, DateTime.Now, true, true, SearchCriteria.DateResolution.Year).Compile();

            var criteriaNotFound = _searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("YearCreated", DateTime.Now.AddYears(-2), DateTime.Now.AddYears(-1), true, true).Compile();

            ////Act
            var results = _searcher.Search(filter);
            var resultsNotFound = _searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Date.Month structure
        /// </summary>
        [Test]
        public void DataTypesTests_Date_Range_Month_SimpleIndexSet()
        {
            ////Arrange

            var criteria = _searcher.CreateSearchCriteria();
            var filter = criteria.Range("MonthCreated", _reIndexDateTime, DateTime.Now, true, true, SearchCriteria.DateResolution.Month).Compile();

            var criteriaNotFound = _searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("MonthCreated", _reIndexDateTime.AddMonths(-2), _reIndexDateTime.AddMonths(-1), true, true).Compile();

            ////Act
            var results = _searcher.Search(filter);
            var resultsNotFound = _searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Date.Day structure
        /// </summary>
        [Test]
        public void DataTypesTests_Date_Range_Day_SimpleIndexSet()
        {
            ////Arrange

            var criteria = _searcher.CreateSearchCriteria();
            var filter = criteria.Range("DayCreated", _reIndexDateTime, DateTime.Now, true, true, SearchCriteria.DateResolution.Day).Compile();

            var criteriaNotFound = _searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("DayCreated", _reIndexDateTime.AddDays(-2), _reIndexDateTime.AddDays(-1), true, true).Compile();

            ////Act
            var results = _searcher.Search(filter);
            var resultsNotFound = _searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Date.Hour structure
        /// </summary>
        [Test]
        public void DataTypesTests_Date_Range_Hour_SimpleIndexSet()
        {
            ////Arrange

            var criteria = _searcher.CreateSearchCriteria();
            var filter = criteria.Range("HourCreated", _reIndexDateTime, DateTime.Now, true, true, SearchCriteria.DateResolution.Hour).Compile();

            var criteriaNotFound = _searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("HourCreated", _reIndexDateTime.AddHours(-2), _reIndexDateTime.AddHours(-1), true, true).Compile();

            ////Act
            var results = _searcher.Search(filter);
            var resultsNotFound = _searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        


        

        


        private static ISearcher _searcher;
        private static IIndexer _indexer;
        private static DateTime _reIndexDateTime;
		private Lucene.Net.Store.Directory _luceneDir;

        [SetUp]
		public  void TestSetup()
        {

			_luceneDir = new RAMDirectory();
			_indexer = IndexInitializer.GetSimpleIndexer(_luceneDir);
            
            _reIndexDateTime = DateTime.Now;
            Thread.Sleep(1000);

            _indexer.RebuildIndex();

			_searcher = IndexInitializer.GetLuceneSearcher(_luceneDir);
        }

        [TearDown]
		public void TestTearDown()
		{
			_luceneDir.Dispose();
		}

    }
}
