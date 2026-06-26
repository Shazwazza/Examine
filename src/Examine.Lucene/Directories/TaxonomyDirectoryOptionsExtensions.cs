namespace Examine.Lucene.Directories
{
    /// <summary>
    /// Internal helpers for determining taxonomy support consistently across the index and the
    /// replication/synced directory factory.
    /// </summary>
    internal static class TaxonomyDirectoryOptionsExtensions
    {
        /// <summary>
        /// Determines whether taxonomy/faceting is both <em>requested</em> and <em>supported</em>.
        /// </summary>
        /// <remarks>
        /// This is the single source of truth shared by <see cref="Providers.LuceneIndex"/> and
        /// <see cref="SyncedFileSystemDirectoryFactory"/> so they can never disagree about whether a
        /// taxonomy writer will exist. <c>UseTaxonomyIndex</c> expresses intent while
        /// <see cref="ITaxonomyDirectoryFactory"/> expresses capability; taxonomy is only effective when
        /// both are present. Without this unified check the replicator could expect a taxonomy writer
        /// that the index never creates (split-brain), throwing on every commit (issue #452).
        /// </remarks>
        public static bool IsTaxonomySupported(this LuceneDirectoryIndexOptions options)
            => options.UseTaxonomyIndex && options.DirectoryFactory is ITaxonomyDirectoryFactory;
    }
}
