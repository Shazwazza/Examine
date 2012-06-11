using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace Examine.Web.Demo.Models
{
    public class TestModel
    {
        [Key]
        public int MyId { get; set; }
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public string Column3 { get; set; }
        public string Column4 { get; set; }
        public string Column5 { get; set; }
        public string Column6 { get; set; }

    }

    public class MyDbContext : DbContext
    {
        public DbSet<TestModel> TestModels { get; set; }
    }


}