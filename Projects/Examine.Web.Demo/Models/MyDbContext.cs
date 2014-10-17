using System.Data.Entity;

namespace Examine.Web.Demo.Models
{
    public class MyDbContext : DbContext
    {
        public DbSet<TestModel> TestModels { get; set; }
    }
}