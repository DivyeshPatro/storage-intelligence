namespace StorageIntelligence.Accounting;

/// <summary>One directory entry that has a file ID, recorded for hard-link reconciliation.</summary>
public readonly record struct LinkRecord(
    FileId128 FileId,
    DirectoryId Directory,
    long LogicalSize,
    long AllocatedSize,
    MeasurementCaveats EffectiveFlags,
    int ExtensionId,
    uint LinkCount)
{
    /// <summary>Serialized size in bytes.</summary>
    public const int SerializedSize = 8 + 8 + 8 + 8 + 8 + 4 + 4 + 4;

    internal static int Compare(LinkRecord a, LinkRecord b)
    {
        int c = a.FileId.CompareTo(b.FileId);
        return c != 0 ? c : a.Directory.Value.CompareTo(b.Directory.Value);
    }

    internal void Write(BinaryWriter writer)
    {
        writer.Write(FileId.High);
        writer.Write(FileId.Low);
        writer.Write(Directory.Value);
        writer.Write(LogicalSize);
        writer.Write(AllocatedSize);
        writer.Write((uint)EffectiveFlags);
        writer.Write(ExtensionId);
        writer.Write(LinkCount);
    }

    internal static LinkRecord Read(BinaryReader reader) => new(
        new FileId128(reader.ReadUInt64(), reader.ReadUInt64()),
        new DirectoryId(reader.ReadInt64()),
        reader.ReadInt64(),
        reader.ReadInt64(),
        (MeasurementCaveats)reader.ReadUInt32(),
        reader.ReadInt32(),
        reader.ReadUInt32());
}

/// <summary>Storage for sorted runs. Each run is written once, then read once.</summary>
public interface IRunStore : IDisposable
{
    /// <summary>Creates a new writable run stream. The store owns the stream.</summary>
    Stream CreateRun();

    /// <summary>Opens every run created so far for reading, positioned at the start.</summary>
    IReadOnlyList<Stream> OpenRunsForReading();

    /// <summary>Total bytes written to runs.</summary>
    long BytesWritten { get; }
}

/// <summary>Keeps runs in memory. For tests and small scans.</summary>
public sealed class InMemoryRunStore : IRunStore
{
    private readonly List<MemoryStream> _runs = [];

    public long BytesWritten => _runs.Sum(r => r.Length);

    public Stream CreateRun()
    {
        var stream = new MemoryStream();
        _runs.Add(stream);
        return stream;
    }

    public IReadOnlyList<Stream> OpenRunsForReading()
    {
        foreach (MemoryStream run in _runs)
        {
            run.Position = 0;
        }

        return _runs;
    }

    public void Dispose()
    {
        foreach (MemoryStream run in _runs)
        {
            run.Dispose();
        }

        _runs.Clear();
    }
}

/// <summary>
/// Keeps runs in delete-on-close temporary files inside a caller-chosen directory, so they are removed
/// even if the process exits unexpectedly (on Windows the OS deletes them when the handle closes).
/// </summary>
public sealed class TempFileRunStore : IRunStore
{
    private readonly string _directory;
    private readonly List<FileStream> _runs = [];

    public TempFileRunStore(string directory)
    {
        ArgumentException.ThrowIfNullOrEmpty(directory);
        _directory = directory;
    }

    public long BytesWritten => _runs.Sum(r => r.Length);

    public Stream CreateRun()
    {
        string path = Path.Combine(_directory, $"si-links-{Guid.NewGuid():N}.run");
        var stream = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.None,
            bufferSize: 64 * 1024,
            FileOptions.DeleteOnClose | FileOptions.SequentialScan);
        _runs.Add(stream);
        return stream;
    }

    public IReadOnlyList<Stream> OpenRunsForReading()
    {
        foreach (FileStream run in _runs)
        {
            run.Flush();
            run.Position = 0;
        }

        return _runs;
    }

    public void Dispose()
    {
        foreach (FileStream run in _runs)
        {
            run.Dispose();
        }

        _runs.Clear();
    }
}

/// <summary>
/// Collects <see cref="LinkRecord"/>s with bounded memory: records are buffered, sorted by file ID and
/// spilled to the run store whenever the buffer fills. <see cref="EnumerateGroups"/> merges the runs.
/// </summary>
public sealed class HardLinkRunWriter
{
    private readonly IRunStore _store;
    private readonly LinkRecord[] _buffer;
    private int _count;
    private bool _finished;

    public HardLinkRunWriter(IRunStore store, int bufferCapacity = 256 * 1024)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentOutOfRangeException.ThrowIfLessThan(bufferCapacity, 1);
        _store = store;
        _buffer = new LinkRecord[bufferCapacity];
    }

    public long RecordsWritten { get; private set; }

    public int RunCount { get; private set; }

    public void Add(in LinkRecord record)
    {
        if (_finished)
        {
            throw new InvalidOperationException("The writer has been finished.");
        }

        _buffer[_count++] = record;
        RecordsWritten++;
        if (_count == _buffer.Length)
        {
            Spill();
        }
    }

    /// <summary>Spills any buffered records. No more records may be added afterwards.</summary>
    public void Finish()
    {
        if (!_finished)
        {
            Spill();
            _finished = true;
        }
    }

    /// <summary>
    /// Yields every file ID's records in ascending file-ID order. The yielded list is reused between
    /// iterations; callers must not keep it.
    /// </summary>
    public IEnumerable<IReadOnlyList<LinkRecord>> EnumerateGroups()
    {
        if (!_finished)
        {
            throw new InvalidOperationException("Call Finish before enumerating groups.");
        }

        IReadOnlyList<Stream> runs = _store.OpenRunsForReading();
        var readers = new RunReader[runs.Count];
        var queue = new PriorityQueue<int, LinkRecord>(Comparer<LinkRecord>.Create(LinkRecord.Compare));
        for (int i = 0; i < runs.Count; i++)
        {
            readers[i] = new RunReader(runs[i]);
            if (readers[i].TryRead(out LinkRecord first))
            {
                queue.Enqueue(i, first);
            }
        }

        try
        {
            var group = new List<LinkRecord>();
            while (queue.TryDequeue(out int runIndex, out LinkRecord record))
            {
                if (group.Count > 0 && group[0].FileId != record.FileId)
                {
                    yield return group;
                    group.Clear();
                }

                group.Add(record);
                if (readers[runIndex].TryRead(out LinkRecord next))
                {
                    queue.Enqueue(runIndex, next);
                }
            }

            if (group.Count > 0)
            {
                yield return group;
            }
        }
        finally
        {
            foreach (RunReader reader in readers)
            {
                reader?.Dispose();
            }
        }
    }

    private void Spill()
    {
        if (_count == 0)
        {
            return;
        }

        Array.Sort(_buffer, 0, _count, Comparer<LinkRecord>.Create(LinkRecord.Compare));
        Stream stream = _store.CreateRun();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(_count);
            for (int i = 0; i < _count; i++)
            {
                _buffer[i].Write(writer);
            }
        }

        Array.Clear(_buffer, 0, _count);
        _count = 0;
        RunCount++;
    }

    private sealed class RunReader(Stream stream) : IDisposable
    {
        private readonly BinaryReader _reader = new(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        private int _remaining = -1;

        public bool TryRead(out LinkRecord record)
        {
            if (_remaining < 0)
            {
                _remaining = _reader.ReadInt32();
            }

            if (_remaining == 0)
            {
                record = default;
                return false;
            }

            _remaining--;
            record = LinkRecord.Read(_reader);
            return true;
        }

        public void Dispose() => _reader.Dispose();
    }
}
