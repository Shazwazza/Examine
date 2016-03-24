using System;

namespace Examine.Web.Demo.Models
{
    public class TestModel
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Manufacturer { get; set; }
        public int MegaPixels { get; set; }
        public string Model { get; set; }
        public string Description { get; set; }

        public decimal Price { get; set; }
        public DateTime ReleaseDate { get; set; }

        public string Color { get; set; }

        public int InStock { get; set; }
    }
}