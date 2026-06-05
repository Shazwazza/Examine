using System.Collections.Generic;
using System.Threading.Tasks;

namespace Examine
{
    /// <summary>
    /// Represents stats for a <see cref="IIndex"/>
    /// </summary>
    public interface IIndexStats
    {
        /// <summary>
        /// Gets the ammount of documents in the index
        /// </summary>
        /// <returns></returns>
        long GetDocumentCount();

        /// <summary>
        /// Gets the field names in the index
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetFieldNames();
    }
}
