using System.Threading.Channels;
using StorageIntelligence.Accounting;
using StorageIntelligence.Scanning.Windows.Interop;

namespace StorageIntelligence.Scanning.Windows;

/// <summary>Scan pacing. Both modes must produce identical results on a stable volume.</summary>
public enum ScanMode
{
    /// <summary>One listing worker at background (low I/O and CPU) priority. The default.</summary>
    LowImpact,

    /// <summary>Several listing workers at normal priority.</summary>
    Fast,
}

/// <summary>Which files get their hard-link count measured (one metadata-only open per file).</summary>
public enum LinkCountMeasurement
{
    /// <summary>Never measured; files with every observed link inside a folder are "link status unknown".</summary>
    None,

    /// <summary>Measured for retained (large) files, the ones shown in largest-file listings.</summary>
    RetainedFiles,

    /// <summary>Measured for every file. Slow; used for validation.</summary>
    All,
}

public sealed record ScanOptions
{
    public ScanMode Mode { get; init; } = ScanMode.LowImpact;

    public int FastWorkers { get; init; } = Math.Clamp(Environment.ProcessorCount, 2, 4);

    public LinkCountMeasurement LinkCounts { get; init; } = LinkCountMeasurement.RetainedFiles;

    public long RetainFileThreshold { get; init; } = 10L * 1024 * 1024;

    /// <summary>Directory for hard-link run files. Null keeps runs in memory (small scans and tests only).</summary>
    public string? TempDirectory { get; init; }

    /// <summary>Free space that must remain on the temp volume; below this, reconciliation is abandoned.</summary>
    public long TempReserveBytes { get; init; } = 512L * 1024 * 1024;

    public int LinkRunBufferRecords { get; init; } = 256 * 1024;

    /// <summary>Overrides the policy derived from the volume (tests and validation only).</summary>
    public MeasurementPolicy? PolicyOverride { get; init; }

    /// <summary>Called for every file observation with its root-relative path (validation only).</summary>
    public Action<string, FileObservation, MeasurementCaveats>? FileObserver { get; init; }

    /// <summary>Called after a directory has been opened and before its first batch is read (tests only).</summary>
    public Action<string>? AfterDirectoryOpened { get; init; }

    /// <summary>Maximum number of path/error records kept in the report; counts are always complete.</summary>
    public int MaxErrorRecords { get; init; } = 1000;
}

/// <summary>A path the scan could not fully cover.</summary>
public sealed record ScanIssue(string RelativePath, string Kind, int Win32Error);

public sealed record ScanResult(
    string Root,
    VolumeInfo Volume,
    ScanOptions Options,
    DateTime StartedUtc,
    TimeSpan Duration,
    ScanAccounting Accounting,
    InMemoryScanStore Store,
    IReadOnlyList<ScanIssue> Issues,
    long IssueCount,
    long LinkCountFailures,
    IReadOnlyDictionary<DirectoryId, string> DirectoryPaths,
    long AllocationCorrections = 0);

/// <summary>
/// Read-only Windows scanner. Listing workers read directories; a single consumer (the calling thread) owns the
/// <see cref="ScanAggregator"/>. Reparse points are never traversed, file contents are never read and no handle
/// with data, write or delete access is ever opened.
/// </summary>
public sealed class WindowsScanner
{
    private const int BatchEntries = 4096;


    /// <summary>Pseudo Win32 error recorded when a listing worker failed unexpectedly.</summary>
    public const int UnexpectedWorkerError = -1;

    private sealed record WorkItem(DirectoryId Id, string RelativePath, string ExtendedPath);

    private sealed record Batch(
        WorkItem Work,
        List<RawEntry> Entries,
        bool Opened,
        int OpenError,
        bool Final,
        int ListingError,
        bool DirectoryChanged);

    public static ScanResult Scan(string root, ScanOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);
        ArgumentNullException.ThrowIfNull(options);

        DateTime started = DateTime.UtcNow;
        long scanStartFileTime = started.ToFileTimeUtc();
        var clock = System.Diagnostics.Stopwatch.StartNew();
        VolumeInfo volume = VolumeInfo.Read(root);
        MeasurementPolicy policy = options.PolicyOverride ?? volume.Policy;

        var store = new InMemoryScanStore();
        using IRunStore runs = options.TempDirectory is null ? new InMemoryRunStore() : new TempFileRunStore(options.TempDirectory);
        HardLinkRunWriter? links = null;
        if (policy.FileSystemSupportsHardLinks)
        {
            TempSpaceGuard? guard = options.TempDirectory is null
                ? null
                : new TempSpaceGuard(() => FreeBytes(options.TempDirectory), options.TempReserveBytes);
            links = new HardLinkRunWriter(runs, options.LinkRunBufferRecords, guard);
        }

        var aggregator = new ScanAggregator(
            new AggregatorOptions { Policy = policy, RetainFileThreshold = options.RetainFileThreshold }, store, links);

        string rootExtended = Native.ToExtendedPath(root).TrimEnd('\\');
        var paths = new Dictionary<DirectoryId, string>();
        var issues = new List<ScanIssue>();
        long issueCount = 0, linkCountFailures = 0, allocationCorrections = 0;
        ListingFormat format = policy.FileSystemSupportsHardLinks ? ListingFormat.IdExtended : ListingFormat.Full;

        int workers = options.Mode == ScanMode.LowImpact ? 1 : Math.Max(1, options.FastWorkers);
        int maxInFlight = workers * 2;
        var work = Channel.CreateUnbounded<WorkItem>(new UnboundedChannelOptions { SingleWriter = true });
        var batches = Channel.CreateBounded<Batch>(new BoundedChannelOptions(64) { SingleReader = true, FullMode = BoundedChannelFullMode.Wait });
        using var stopWorkers = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var threads = new Thread[workers];
        for (int i = 0; i < workers; i++)
        {
            threads[i] = new Thread(() => WorkerLoop(work.Reader, batches.Writer, options, format, scanStartFileTime, stopWorkers.Token))
            {
                IsBackground = true,
                Name = $"si-scan-{i}",
            };
            threads[i].Start();
        }

        var pending = new Stack<WorkItem>();
        int inFlight = 0;
        DirectoryId rootId = aggregator.BeginRoot(root);
        paths[rootId] = "";
        pending.Push(new WorkItem(rootId, "", rootExtended + "\\"));
        bool cancelled = false;

        try
        {
            while (pending.Count > 0 || inFlight > 0)
            {
                while (inFlight < maxInFlight && pending.Count > 0)
                {
                    work.Writer.TryWrite(pending.Pop());
                    inFlight++;
                }

                Batch batch;
                try
                {
                    batch = ReadBatch(batches.Reader, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    cancelled = true;
                    break;
                }

                Apply(batch);
            }
        }
        finally
        {
            work.Writer.TryComplete();
            stopWorkers.Cancel();
            foreach (Thread t in threads)
            {
                t.Join();
            }
        }

        AggregationOutcome outcome = cancelled ? aggregator.Cancel() : aggregator.Complete();
        ScanAccounting accounting = ScanAccounting.Build(outcome, store, links, policy);
        return new ScanResult(
            root, volume, options, started, clock.Elapsed, accounting, store, issues, issueCount, linkCountFailures, paths, allocationCorrections);

        void Apply(Batch batch)
        {
            WorkItem item = batch.Work;
            if (!batch.Opened)
            {
                aggregator.MarkInaccessible(item.Id);
                Issue(item.RelativePath, "inaccessible", batch.OpenError);
                aggregator.EndListing(item.Id);
                inFlight--;
                return;
            }

            foreach (RawEntry entry in batch.Entries)
            {
                string childRelative = item.RelativePath.Length == 0 ? entry.Name : item.RelativePath + "\\" + entry.Name;
                if (entry.IsDirectory)
                {
                    if (entry.IsReparsePoint)
                    {
                        if (Native.IsNameSurrogate(entry.ReparseTag))
                        {
                            aggregator.AddNotFollowedLink(item.Id, entry.Name, LinkKindOf(entry.ReparseTag));
                        }
                        else
                        {
                            aggregator.AddSkippedReparseDirectory(item.Id, entry.Name);
                            Issue(childRelative, $"skipped-reparse-directory:0x{entry.ReparseTag:X8}", 0);
                        }

                        continue;
                    }

                    DirectoryId child = aggregator.DiscoverDirectory(item.Id, entry.Name);
                    paths[child] = childRelative;
                    pending.Push(new WorkItem(child, childRelative, item.ExtendedPath + entry.Name + "\\"));
                    continue;
                }

                if (entry.IsReparsePoint && Native.IsNameSurrogate(entry.ReparseTag))
                {
                    aggregator.AddNotFollowedLink(item.Id, entry.Name, LinkKindOf(entry.ReparseTag));
                    continue;
                }

                FileObservation observation = Observe(entry, item.ExtendedPath + entry.Name, policy, options, scanStartFileTime, ref linkCountFailures, ref allocationCorrections);
                aggregator.AddFile(item.Id, observation);
                options.FileObserver?.Invoke(childRelative, observation, policy.Effective(observation.Flags, observation.FileId.HasValue));
            }

            if (!batch.Final)
            {
                return;
            }

            if (batch.ListingError != 0)
            {
                aggregator.MarkIncomplete(item.Id);
                Issue(item.RelativePath, "listing-incomplete", batch.ListingError);
            }

            if (batch.DirectoryChanged)
            {
                aggregator.MarkChanged(item.Id);
                Issue(item.RelativePath, "changed-during-scan", 0);
            }

            aggregator.EndListing(item.Id);
            inFlight--;
        }

        void Issue(string relativePath, string kind, int error)
        {
            issueCount++;
            if (issues.Count < options.MaxErrorRecords)
            {
                issues.Add(new ScanIssue(relativePath, kind, error));
            }
        }
    }

    private static Batch ReadBatch(ChannelReader<Batch> reader, CancellationToken token)
    {
        Batch? batch;
        while (!reader.TryRead(out batch))
        {
            if (!reader.WaitToReadAsync(token).AsTask().GetAwaiter().GetResult())
            {
                throw new InvalidOperationException("Workers stopped unexpectedly.");
            }
        }

        return batch;
    }

    private static void WorkerLoop(
        ChannelReader<WorkItem> work,
        ChannelWriter<Batch> output,
        ScanOptions options,
        ListingFormat format,
        long scanStartFileTime,
        CancellationToken token)
    {
        bool background = options.Mode == ScanMode.LowImpact &&
            Native.SetThreadPriority(Native.GetCurrentThread(), Native.THREAD_MODE_BACKGROUND_BEGIN);
        try
        {
            while (!token.IsCancellationRequested)
            {
                WorkItem? item;
                try
                {
                    if (!work.WaitToReadAsync(token).AsTask().GetAwaiter().GetResult())
                    {
                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                if (!work.TryRead(out item))
                {
                    continue;
                }

                try
                {
                    ListDirectory(item, output, options, format, token);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    // Never leave the consumer waiting: report the directory as an incomplete listing.
                    try
                    {
                        output.WriteAsync(new Batch(item, [], true, 0, true, UnexpectedWorkerError, false), token).AsTask().GetAwaiter().GetResult();
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }
            }
        }
        finally
        {
            if (background)
            {
                Native.SetThreadPriority(Native.GetCurrentThread(), Native.THREAD_MODE_BACKGROUND_END);
            }
        }
    }

    private static void ListDirectory(WorkItem item, ChannelWriter<Batch> output, ScanOptions options, ListingFormat format, CancellationToken token)
    {
        using DirectoryReader? reader = DirectoryReader.Open(item.ExtendedPath, format, out int openError);
        if (reader is null)
        {
            Write(new Batch(item, [], false, openError, true, 0, false));
            return;
        }

        bool haveBefore = reader.TryGetDirectoryInfo(out long writeBefore, out long changeBefore);
        options.AfterDirectoryOpened?.Invoke(item.RelativePath);

        var entries = new List<RawEntry>(256);
        while (reader.ReadBatch(entries))
        {
            if (entries.Count >= BatchEntries)
            {
                Write(new Batch(item, entries, true, 0, false, 0, false));
                entries = new List<RawEntry>(256);
            }
        }

        bool changed = haveBefore &&
            reader.TryGetDirectoryInfo(out long writeAfter, out long changeAfter) &&
            (writeAfter != writeBefore || changeAfter != changeBefore);
        Write(new Batch(item, entries, true, 0, true, reader.Error, changed));

        void Write(Batch batch)
        {
            try
            {
                output.WriteAsync(batch, token).AsTask().GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                // The consumer is cancelling; the remaining work is reported as cancelled by the aggregator.
            }
        }
    }

    private static FileObservation Observe(
        RawEntry entry,
        string extendedPath,
        MeasurementPolicy policy,
        ScanOptions options,
        long scanStartFileTime,
        ref long linkCountFailures,
        ref long allocationCorrections)
    {
        MeasurementCaveats caveats = MeasurementCaveats.None;
        if ((entry.Attributes & Native.FILE_ATTRIBUTE_SPARSE_FILE) != 0)
        {
            caveats |= MeasurementCaveats.Sparse;
        }

        if ((entry.Attributes & Native.FILE_ATTRIBUTE_COMPRESSED) != 0)
        {
            caveats |= MeasurementCaveats.Compressed;
        }

        if ((entry.Attributes & (Native.FILE_ATTRIBUTE_OFFLINE | Native.FILE_ATTRIBUTE_RECALL_ON_OPEN | Native.FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS)) != 0)
        {
            caveats |= MeasurementCaveats.CloudPlaceholder;
        }

        if (entry.IsReparsePoint)
        {
            caveats |= entry.ReparseTag switch
            {
                Native.IO_REPARSE_TAG_WOF => MeasurementCaveats.WofCompressed,
                _ when (entry.ReparseTag & Native.IO_REPARSE_TAG_CLOUD_MASK) == Native.IO_REPARSE_TAG_CLOUD => MeasurementCaveats.CloudPlaceholder,
                _ => MeasurementCaveats.UnrecognizedAttributes,
            };
        }

        if (entry.ChangeTime >= scanStartFileTime || entry.LastWriteTime >= scanStartFileTime)
        {
            caveats |= MeasurementCaveats.ChangedDuringScan;
        }

        long allocated = entry.AllocationSize;
        bool anomalous = !entry.IsReparsePoint &&
            (entry.Attributes & (Native.FILE_ATTRIBUTE_SPARSE_FILE | Native.FILE_ATTRIBUTE_COMPRESSED)) == 0 &&
            entry.AllocationSize < entry.EndOfFile;
        if (anomalous)
        {
            // A plain file always has allocation >= size. Less means the listing does not show the real storage:
            // WOF-compressed files are listed with allocation 0 and no reparse attribute (M0b evidence). Ask the file.
            allocationCorrections++;
            using var handle = Native.OpenMetadata(extendedPath, Native.FILE_READ_ATTRIBUTES, out _);
            if (!handle.IsInvalid && Native.TryGetStandardInfo(handle, out Native.FILE_STANDARD_INFO info))
            {
                allocated = info.AllocationSize;
                caveats |= Native.IsExternallyBacked(handle) == true
                    ? MeasurementCaveats.WofCompressed
                    : MeasurementCaveats.UnrecognizedAttributes;
            }
            else
            {
                caveats |= MeasurementCaveats.UnrecognizedAttributes;
            }
        }

        FileId128? id = policy.FileSystemSupportsHardLinks ? entry.FileId : null;
        uint linkCount = 0;
        bool measure = id.HasValue && (options.LinkCounts == LinkCountMeasurement.All ||
            (options.LinkCounts == LinkCountMeasurement.RetainedFiles && allocated >= options.RetainFileThreshold));
        if (measure)
        {
            using var handle = Native.OpenMetadata(extendedPath, Native.FILE_READ_ATTRIBUTES, out _);
            if (!handle.IsInvalid && Native.TryGetStandardInfo(handle, out Native.FILE_STANDARD_INFO info))
            {
                linkCount = info.NumberOfLinks;
                if (info.AllocationSize != allocated || info.EndOfFile != entry.EndOfFile)
                {
                    caveats |= MeasurementCaveats.ChangedDuringScan;
                }
            }
            else
            {
                linkCountFailures++;
            }
        }

        return new FileObservation(entry.Name, id, entry.EndOfFile, allocated, caveats, linkCount);
    }

    private static LinkKind LinkKindOf(uint tag) => tag switch
    {
        Native.IO_REPARSE_TAG_MOUNT_POINT => LinkKind.Junction,
        Native.IO_REPARSE_TAG_SYMLINK => LinkKind.SymbolicLink,
        _ => LinkKind.Other,
    };

    private static long FreeBytes(string directory) =>
        Native.GetDiskFreeSpaceEx(directory, out ulong available, out _, out _) ? (long)available : 0;
}
