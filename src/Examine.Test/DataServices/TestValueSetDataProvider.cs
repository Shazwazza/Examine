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
    public class TestValueSetDataProvider : IValueSetDataService
    {
        private static int m_CurrentId = 0;
        private static readonly object m_Locker = new object();

        #region ISimpleDataService Members

        /// <summary>
        /// Return a dummy data structure for the different types of indexes
        /// </summary>
        /// <param name="indexType"></param>
        /// <returns></returns>
        public IEnumerable<ValueSet> GetAllData(string indexType)
        {
            switch (indexType)
            {
                case "Pictures":
                    return new List<ValueSet>()
                    {
                        CreateNewPicture(),
                        CreateNewPicture(),
                        CreateNewPicture()
                    };
                case "Documents":
                    return new List<ValueSet>()
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
        public ValueSet CreateNewPicture()
        {
            lock (m_Locker)
            {
                var now = DateTime.Now;
                return new ValueSet((++m_CurrentId).ToString(), "Pictures", new Dictionary<string, object>()
                {
                    {"Title", Guid.NewGuid().ToString()},
                    {"Photographer", Guid.NewGuid()},
                    {"YearCreated", now},
                    {"MonthCreated", now},
                    {"DayCreated", now},
                    {"HourCreated", now},
                    {"MinuteCreated", now},
                });
            }
        }


        /// <summary>
        /// Create a new Document data set with a newly incremented id
        /// </summary>
        /// <returns></returns>
        public ValueSet CreateNewDocument(DateTime created)
        {
            lock (m_Locker)
            {
                return new ValueSet((++m_CurrentId).ToString(), "Documents", new Dictionary<string, object>()
                {
                    {"Author", Guid.NewGuid().ToString()},
                    {"DateCreated", created.ToString("yyyy-MM-dd HH:mm:ss")},
                    {"YearCreated", created.ToString("yyyy-MM-dd HH:mm:ss")},
                    {"MonthCreated", created.ToString("yyyy-MM-dd HH:mm:ss")},
                    {"DayCreated", created.ToString("yyyy-MM-dd HH:mm:ss")},
                    {"HourCreated", created.ToString("yyyy-MM-dd HH:mm:ss")},
                    {"MinuteCreated", created.ToString("yyyy-MM-dd HH:mm:ss")},
                    {"SomeNumber", new Random().Next(1, 100)},
                    {"SomeFloat", new Random().Next(1, 100)},
                    {"SomeDouble", new Random().Next(1, 100)},
                    {"SomeLong", new Random().Next(1, 100)},
                });
            }
        }

        #endregion
    }
}
