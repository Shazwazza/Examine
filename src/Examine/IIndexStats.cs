using System.Collections.Generic;
using System.Threading.Tasks;

namespace Examine
{
    internal interface IIndexStats
    {
        Task<long> GetDocumentCountAsync();
        Task<IEnumerable<string>> GetFieldNamesAsync();
    }
}
