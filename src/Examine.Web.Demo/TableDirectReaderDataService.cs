using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using Examine.LuceneEngine;
using Examine.Web.Demo.Models;

namespace Examine.Web.Demo
{
    /// <summary>
    /// Data service for Examine using SqlCe's DirectTable reader as it is by far the fastest 
    /// way to read data from SqlCe.
    /// </summary>
    public class TableDirectReaderDataService : IValueSetDataService
    {
        /// <summary>
        /// Returns some random items from the storage
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ValueSet> GetRandomItems()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var current = 0;

            using (var db = new MyDbContext())
            {
                using (db.Database.Connection)
                {
                    db.Database.Connection.Open();
                    using (var cmd = (SqlCeCommand)db.Database.Connection.CreateCommand())
                    {
                        cmd.CommandText = "TestModels";
                        cmd.CommandType = CommandType.TableDirect;
                        var rs = cmd.ExecuteResultSet(ResultSetOptions.None);
                        while (rs.Read())
                        {
                            //A 1 in 1000 chance of being returned
                            if (random.Next(1, 1001) == 1000)
                            {
                                yield return new ValueSet(rs.GetInt32(0).ToString(), "TestType", new Dictionary<string, object>()
                                {
                                    {"Column1", rs.GetString(1)},
                                    {"Column2", rs.GetString(2)},
                                    {"Column3", rs.GetString(3)},
                                    {"Column4", rs.GetString(4)},
                                    {"Column5", rs.GetString(5)},
                                    {"Column6", rs.GetString(6)}
                                });
                            }
                            current++;
                        }
                    }
                }
            }
        }

        public IEnumerable<ValueSet> GetAllData(string indexType)
        {
            using (var db = new MyDbContext())
            {
                using (db.Database.Connection)
                {
                    db.Database.Connection.Open();
                    using (var cmd = (SqlCeCommand)db.Database.Connection.CreateCommand())
                    {
                        cmd.CommandText = "TestModels";
                        cmd.CommandType = CommandType.TableDirect;
                        var rs = cmd.ExecuteResultSet(ResultSetOptions.None);
                        while(rs.Read())
                        {
                            yield return new ValueSet(rs.GetInt32(0).ToString(), "TestType", new Dictionary<string, object>()
                            {
                                {"Column1", rs.GetString(1)},
                                {"Column2", rs.GetString(2)},
                                {"Column3", rs.GetString(3)},
                                {"Column4", rs.GetString(4)},
                                {"Column5", rs.GetString(5)},
                                {"Column6", rs.GetString(6)}
                            });
                        }
                    }                
                }
            }
        }
    }
}