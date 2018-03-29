using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace Examine.Web.Demo.Models
{
    public class TestModel
    {
    }

    public class MyDbContext : DbContext
    {
        public DbSet<TestModel> TestModels { get; set; }
    }

    public class IndexInfo
    {
        public int Docs { get; set; }
        public int Fields { get; set; }
    }

}