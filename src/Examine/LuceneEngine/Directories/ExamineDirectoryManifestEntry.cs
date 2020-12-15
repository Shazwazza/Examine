using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.LuceneEngine.Directories
{
    public class ExamineDirectoryManifestEntry
    {
        public string LuceneFileName { get; set; }

        public string BlobFileName { get; set; }

        public long ModifiedDate { get; set; }

        public long Length { get; set; }

        public string OriginalManifestId { get; set; }
    }
}
