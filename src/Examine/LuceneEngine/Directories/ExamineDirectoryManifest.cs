using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.LuceneEngine.Directories
{
    public class ExamineDirectoryManifest
    {
        public long Modified { get; set; }

        public string Id { get; set; }

        public List<ExamineDirectoryManifestEntry> Entries { get; set; }
    }
}
