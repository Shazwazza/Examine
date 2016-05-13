using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine;

namespace Examine.Test.DataServices
{
    /// <summary>
    /// Used for testing. Supports 2 index types: Pictures, Documents
    /// </summary>
    public class TestSimpleDataProvider : ISimpleDataService
    {
        private static int m_CurrentId = 0;
        private static readonly object m_Locker = new object();

        #region ISimpleDataService Members

        /// <summary>
        /// Return a dummy data structure for the different types of indexes
        /// </summary>
        /// <param name="indexType"></param>
        /// <returns></returns>
        public IEnumerable<SimpleDataSet> GetAllData(string indexType)
        {
            switch (indexType)
            {
                case "Pictures":
                    return new List<SimpleDataSet>()
                    {
                        CreateNewPicture(),
                        CreateNewPicture(),
                        CreateNewPicture()
                    };
                case "Documents":
                    return new List<SimpleDataSet>()
                    {
                        CreateNewDocument(DateTime.Now),
                        
                        CreateNewDocument(DateTime.Now.AddYears(-2))
                    };
                default:
                    throw new ArgumentException("The indexType specified is invalid");
            }
        }

        /// <summary>
        /// Create a new Picture data set with a newly incremented id
        /// </summary>
        /// <returns></returns>
        public SimpleDataSet CreateNewPicture()
        {
            lock (m_Locker)
            {
                return new SimpleDataSet()
                    {
                        NodeDefinition = new IndexedNode() { NodeId = (++m_CurrentId), Type = "Pictures" },
                        RowData = new Dictionary<string, string>() 
                        {
                            { "Title", Guid.NewGuid().ToString()},
                            { "Photographer", Guid.NewGuid().ToString()},
                            { "YearCreated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                            { "MonthCreated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                            { "DayCreated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                            { "HourCreated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                            { "MinuteCreated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                        }
                    };
            }
        }


        /// <summary>
        /// Create a new Document data set with a newly incremented id
        /// </summary>
        /// <returns></returns>
        public SimpleDataSet CreateNewDocument(DateTime created)
        {
            lock (m_Locker)
            {
                return new SimpleDataSet()
                {
                    NodeDefinition = new IndexedNode() { NodeId = (++m_CurrentId), Type = "Documents" },
                    RowData = new Dictionary<string, string>() 
                    {
                            { "Author", Guid.NewGuid().ToString()},
                            { "DateCreated", created.ToString("yyyy-MM-dd HH:mm:ss")},
                            { "YearCreated", created.ToString("yyyy-MM-dd HH:mm:ss")},
                            { "MonthCreated", created.ToString("yyyy-MM-dd HH:mm:ss")},
                            { "DayCreated", created.ToString("yyyy-MM-dd HH:mm:ss")},
                            { "HourCreated", created.ToString("yyyy-MM-dd HH:mm:ss")},
                            { "MinuteCreated", created.ToString("yyyy-MM-dd HH:mm:ss")},
                            { "SomeNumber", new Random().Next(1, 100).ToString()},
                            { "SomeFloat", new Random().Next(1, 100).ToString()},
                            { "SomeDouble", new Random().Next(1, 100).ToString()},
                            { "SomeLong", new Random().Next(1, 100).ToString()},
                    }
                };
            }
        }

        #endregion
    }
}
