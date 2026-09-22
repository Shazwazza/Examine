---
title: Replication
permalink: /replication
uid: replication
order: 5
---

Replication
===

Replication copies a Lucene index from a source directory to a destination directory. The typical use is keeping a fast local index in sync with an index held in slower or shared storage, so that searching is served from local disk while writes go to the durable copy.

Replication is provided by [`ExamineReplicator`](xref:Examine.Lucene.ExamineReplicator), which wraps the [Lucene.NET Replicator](https://lucenenet.apache.org/docs/4.8.0-beta00018/api/replicator/Lucene.Net.Replicator.html) library.

_**Note**: the destination directory must not have any active writers open to it._

## Requirements

The source index must be configured with an [`IndexDeletionPolicy`](xref:Examine.Lucene.LuceneIndexOptions#Examine_Lucene_LuceneIndexOptions_IndexDeletionPolicy) that retains commit points, otherwise there is nothing for the replicator to publish. A [`SnapshotDeletionPolicy`](https://lucenenet.apache.org/docs/4.8.0-beta00018/api/core/Lucene.Net.Index.SnapshotDeletionPolicy.html) is the usual choice.

```cs
services.AddExamineLuceneIndex("MyIndex", options =>
{
    options.IndexDeletionPolicy = new SnapshotDeletionPolicy(
        new KeepOnlyLastCommitDeletionPolicy());
});
```

## Replicating an index

```cs
using var replicator = new ExamineReplicator(
    replicatorLogger,
    clientLogger,
    sourceIndex,               // LuceneIndex
    sourceDirectory,           // Lucene.Net.Store.Directory
    destinationDirectory,      // Lucene.Net.Store.Directory
    destinationTaxonomyDirectory, // Lucene.Net.Store.Directory, or null
    new DirectoryInfo(tempStoragePath));

// Sync once, on demand
replicator.ReplicateIndex();
```

[`ReplicateIndex()`](xref:Examine.Lucene.ExamineReplicator#Examine_Lucene_ExamineReplicator_ReplicateIndex) performs a single synchronization. It throws an [`InvalidOperationException`](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception) if the destination directory - or the destination taxonomy directory, when taxonomy replication is enabled - is locked.

## Taxonomy replication

When the source index uses the [taxonomy sidecar index](xref:configuration#hierarchical-and-taxonomy-facets-configuration), the taxonomy index must be replicated alongside the main index or facet queries against the destination will not resolve correctly.

Pass a `destinationTaxonomyDirectory` to enable this. Pass `null` when the source index is not using a taxonomy index.

## Scheduled replication

[`StartIndexReplicationOnSchedule(int milliseconds)`](xref:Examine.Lucene.ExamineReplicator#Examine_Lucene_ExamineReplicator_StartIndexReplicationOnSchedule_System_Int32_) subscribes to the source index's [`IndexCommitted`](xref:Examine.Lucene.Providers.LuceneIndex#Examine_Lucene_Providers_LuceneIndex_IndexCommitted) event and publishes a new revision on every commit, with the destination polling on the given interval.

```cs
// Check for new revisions every 5 seconds
replicator.StartIndexReplicationOnSchedule(5000);
```

## Monitoring replication health

Scheduled replication tolerates transient failures - a destination briefly locked by another process, for example - by retrying on the next commit. A persistent failure is a different matter: retrying forever would hide the problem while the destination index silently falls further behind.

[`ExamineReplicator`](xref:Examine.Lucene.ExamineReplicator) therefore counts consecutive publish failures and stops replicating once the limit is reached:

* [`ConsecutiveReplicationFailures`](xref:Examine.Lucene.ExamineReplicator#Examine_Lucene_ExamineReplicator_ConsecutiveReplicationFailures) - how many times in a row publishing a revision has failed. Reset to zero on a successful publish, and when scheduled replication is restarted.
* [`MaxConsecutiveReplicationFailures`](xref:Examine.Lucene.ExamineReplicator#Examine_Lucene_ExamineReplicator_MaxConsecutiveReplicationFailures) - the tolerated number of consecutive failures. Defaults to `5`. A value of `0` or less means replication is never stopped automatically.
* [`IsReplicationHealthy`](xref:Examine.Lucene.ExamineReplicator#Examine_Lucene_ExamineReplicator_IsReplicationHealthy) - `false` once that limit has been hit and replication has been stopped.

When the limit is reached, the replicator unsubscribes from `IndexCommitted` and stops its background update thread. A `Critical` log entry is written, and the destination index is no longer updated until replication is restarted by calling [`StartIndexReplicationOnSchedule`](xref:Examine.Lucene.ExamineReplicator#Examine_Lucene_ExamineReplicator_StartIndexReplicationOnSchedule_System_Int32_) again.

[`IsReplicationHealthy`](xref:Examine.Lucene.ExamineReplicator#Examine_Lucene_ExamineReplicator_IsReplicationHealthy) is intended to be surfaced through whatever health checking you already have, so that a stalled replica is visible rather than silent.

```cs
if (!replicator.IsReplicationHealthy)
{
    // Replication has stopped after
    // replicator.MaxConsecutiveReplicationFailures consecutive failures.
    // Investigate, then restart:
    replicator.StartIndexReplicationOnSchedule(5000);
}
```

## Synced directory factory

For the common case of syncing between a main storage location and a local one, [`SyncedFileSystemDirectoryFactory`](xref:Examine.Lucene.Directories.SyncedFileSystemDirectoryFactory) wires this up for you - it maintains a local index directory that is kept in sync with a main directory, without you having to manage an [`ExamineReplicator`](xref:Examine.Lucene.ExamineReplicator) directly.

```cs
services.AddExamineLuceneIndex<LuceneIndex, SyncedFileSystemDirectoryFactory>("MyIndex");
```
