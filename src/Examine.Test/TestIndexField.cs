using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.Test
{
    public class TestIndexField : IIndexField
    {
        public string Name { get; set; }
        public bool EnableSorting { get; set; }
        public string Type { get; set; }
    }
}
