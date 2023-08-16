using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Examine
{
    /// <summary>
    /// Represents <see cref="ISearchResults"/> with no elements
    /// </summary>
	public sealed class EmptySearchResults : ISearchResults
	{
        private EmptySearchResults()
        {   
        }

        /// <summary>
        /// Gets the static instance
        /// </summary>
	    public static ISearchResults Instance { get; } = new EmptySearchResults();

        /// <inheritdoc/>
        public IEnumerator<ISearchResult> GetEnumerator()
		{
			return Enumerable.Empty<ISearchResult>().GetEnumerator();
		}

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
		{
			return Enumerable.Empty<ISearchResult>().GetEnumerator();
		}

        /// <inheritdoc/>
		public long TotalItemCount => 0;


#pragma warning disable IDE0060 // Remove unused parameter
        /// <inheritdoc/>
        public IEnumerable<ISearchResult> Skip(int skip)
#pragma warning restore IDE0060 // Remove unused parameter
        {
			return Enumerable.Empty<ISearchResult>();
		}

        /// <inheritdoc/>
#pragma warning disable IDE0060 // Remove unused parameter
        public IEnumerable<ISearchResult> SkipTake(int skip, int? take = null)
#pragma warning restore IDE0060 // Remove unused parameter
        {
			return Enumerable.Empty<ISearchResult>();
		}
    }
}
