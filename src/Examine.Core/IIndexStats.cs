using System.Collections.Generic;
using System.Threading.Tasks;

namespace Examine
{
    public interface IIndexStats
    {
        long GetDocumentCount();
        IEnumerable<string> GetFieldNames();
    }
}
