using System.Buffers.Binary;

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
    internal static int Compare(LinkRecord a, LinkRecord b)
    {
        int c = a.FileId.CompareTo(b.FileId);
        return c != 0 ? c : a.Directory.Value.CompareTo(b.Directory.Value);
    }
}

/// <summary>
/// Versioned binary format for one sorted run of <see cref="LinkRecord"/>s.
/// <code>
/// header : "SILR" | u16 version (=1) | u16 reserved (=0) | i32 recordCount
/// record : fileId  — varint highDelta; then varint lowDelta if highDelta == 0, else varint low
///          varint directory | varint logical | varint allocated | varint caveats | varint extension | varint linkCount
/// trailer: "END1" | i32 recordCount
/// </code>
/// Records must be sorted by file ID, so IDs are delta-encoded against the previous record. All integers are
/// unsigned LEB128 varints. Readers reject an unknown version, a bad magic/trailer or a truncated stream.
/// </summary>
public static class LinkRunFormat
{
    public const ushort CurrentVersion = 1;

    /// <summary>Upper bound for one encoded record (two 10-byte IDs, four 10-byte longs, three 5-byte ints).</summary>
    public const int MaxRecordBytes = 10 + 10 + 10 + 10 + 10 + 5 + 5 + 5;

    public const int HeaderBytes = 12;
    public const int TrailerBytes = 8;

    private static ReadOnlySpan<byte> Magic => "SILR"u8;
    private static ReadOnlySpan<byte> TrailerMagic => "END1"u8;

    public static void Write(Stream stream, ReadOnlySpan<LinkRecord> sortedRecords)
    {
        ArgumentNullException.ThrowIfNull(stream);
        Span<byte> header = stackalloc byte[HeaderBytes];
        Magic.CopyTo(header);
        BinaryPrimitives.WriteUInt16LittleEndian(header[4..], CurrentVersion);
        BinaryPrimitives.WriteUInt16LittleEndian(header[6..], 0);
        BinaryPrimitives.WriteInt32LittleEndian(header[8..], sortedRecords.Length);
        stream.Write(header);

        byte[] buffer = new byte[64 * 1024];
        int pos = 0;
        FileId128 previous = default;
        foreach (ref readonly LinkRecord r in sortedRecords)
        {
            if (buffer.Length - pos < MaxRecordBytes)
            {
                stream.Write(buffer, 0, pos);
                pos = 0;
            }

            if (r.FileId < previous)
            {
                throw new ArgumentException("Records must be sorted by file ID.", nameof(sortedRecords));
            }

            ulong highDelta = r.FileId.High - previous.High;
            pos += WriteVarint(buffer.AsSpan(pos), highDelta);
            pos += WriteVarint(buffer.AsSpan(pos), highDelta == 0 ? r.FileId.Low - previous.Low : r.FileId.Low);
            pos += WriteVarint(buffer.AsSpan(pos), checked((ulong)r.Directory.Value));
            pos += WriteVarint(buffer.AsSpan(pos), checked((ulong)r.LogicalSize));
            pos += WriteVarint(buffer.AsSpan(pos), checked((ulong)r.AllocatedSize));
            pos += WriteVarint(buffer.AsSpan(pos), (uint)r.EffectiveFlags);
            pos += WriteVarint(buffer.AsSpan(pos), checked((uint)r.ExtensionId));
            pos += WriteVarint(buffer.AsSpan(pos), r.LinkCount);
            previous = r.FileId;
        }

        stream.Write(buffer, 0, pos);
        Span<byte> trailer = stackalloc byte[TrailerBytes];
        TrailerMagic.CopyTo(trailer);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], sortedRecords.Length);
        stream.Write(trailer);
    }

    /// <summary>Streams records from one run. Throws <see cref="InvalidDataException"/> on any format violation.</summary>
    public sealed class Reader : IDisposable
    {
        private readonly BufferedStream _stream;
        private readonly int _count;
        private int _read;
        private FileId128 _previous;

        public Reader(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            _stream = new BufferedStream(stream, 64 * 1024);
            Span<byte> header = stackalloc byte[HeaderBytes];
            ReadExactly(header);
            if (!header[..4].SequenceEqual(Magic))
            {
                throw new InvalidDataException("Not a hard-link run (bad magic).");
            }

            ushort version = BinaryPrimitives.ReadUInt16LittleEndian(header[4..]);
            if (version != CurrentVersion)
            {
                throw new InvalidDataException($"Unsupported hard-link run version {version}.");
            }

            _count = BinaryPrimitives.ReadInt32LittleEndian(header[8..]);
            if (_count < 0)
            {
                throw new InvalidDataException("Negative record count.");
            }
        }

        public bool TryRead(out LinkRecord record)
        {
            if (_read == _count)
            {
                Span<byte> trailer = stackalloc byte[TrailerBytes];
                ReadExactly(trailer);
                if (!trailer[..4].SequenceEqual(TrailerMagic) || BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]) != _count)
                {
                    throw new InvalidDataException("Hard-link run trailer is missing or inconsistent.");
                }

                _read++; // Trailer consumed; later calls return false without re-reading.
                record = default;
                return false;
            }

            if (_read > _count)
            {
                record = default;
                return false;
            }

            ulong highDelta = ReadVarint();
            ulong lowValue = ReadVarint();
            var id = highDelta == 0
                ? new FileId128(_previous.High, _previous.Low + lowValue)
                : new FileId128(_previous.High + highDelta, lowValue);
            record = new LinkRecord(
                id,
                new DirectoryId(checked((long)ReadVarint())),
                checked((long)ReadVarint()),
                checked((long)ReadVarint()),
                (MeasurementCaveats)checked((uint)ReadVarint()),
                checked((int)ReadVarint()),
                checked((uint)ReadVarint()));
            _previous = id;
            _read++;
            return true;
        }

        public void Dispose() => _stream.Dispose();

        private ulong ReadVarint()
        {
            ulong value = 0;
            for (int shift = 0; shift < 70; shift += 7)
            {
                int b = _stream.ReadByte();
                if (b < 0)
                {
                    throw new InvalidDataException("Hard-link run is truncated.");
                }

                value |= (ulong)(b & 0x7F) << shift;
                if ((b & 0x80) == 0)
                {
                    return value;
                }
            }

            throw new InvalidDataException("Malformed varint in hard-link run.");
        }

        private void ReadExactly(Span<byte> destination)
        {
            try
            {
                _stream.ReadExactly(destination);
            }
            catch (EndOfStreamException e)
            {
                throw new InvalidDataException("Hard-link run is truncated.", e);
            }
        }
    }

    private static int WriteVarint(Span<byte> destination, ulong value)
    {
        int i = 0;
        while (value >= 0x80)
        {
            destination[i++] = (byte)(value | 0x80);
            value >>= 7;
        }

        destination[i++] = (byte)value;
        return i;
    }
}

/// <summary>Storage for sorted runs. Each run is written once, then read once.</summary>
public interface IRunStore : IDisposable
{
    /// <summary>Creates a new writable run stream. The store owns the stream.</summary>
    Stream CreateRun();

    /// <summary>Opens every run created so far for reading, positioned at the start.</summary>
    IReadOnlyList<Stream> OpenRunsForReading();

    /// <summary>Discards every run and releases its space.</summary>
    void Clear();

    /// <summary>Total bytes currently held in runs.</summary>
    long BytesWritten { get; }
}

/// <summary>Keeps runs in memory. For tests and small scans.</summary>
public sealed class InMemoryRunStore : IRunStore
{
    private readonly List<MemoryStream> _runs = [];

    public long BytesWritten => _runs.Sum(r => r.CanSeek ? r.Length : 0); // merged runs are already closed

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

    public void Clear()
    {
        foreach (MemoryStream run in _runs)
        {
            run.Dispose();
        }

        _runs.Clear();
    }

    public void Dispose() => Clear();
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

    public string Directory => _directory;

    public long BytesWritten => _runs.Sum(r => r.CanSeek ? r.Length : 0); // merged runs are already closed

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

    public void Clear()
    {
        foreach (FileStream run in _runs)
        {
            run.Dispose();
        }

        _runs.Clear();
    }

    public void Dispose() => Clear();
}

/// <summary>
/// Protects the temp volume. Before each spill the writer checks that writing the run would leave at least
/// <see cref="ReserveBytes"/> free; if not, reconciliation is abandoned rather than filling the disk.
/// </summary>
/// <param name="AvailableBytes">Returns current free bytes on the temp volume (e.g. DriveInfo.AvailableFreeSpace).</param>
/// <param name="ReserveBytes">Free space that must remain after writing.</param>
public sealed record TempSpaceGuard(Func<long> AvailableBytes, long ReserveBytes)
{
    /// <summary>Planning estimate per record, used only for the preflight; spills use the worst case.</summary>
    public const int EstimatedBytesPerRecord = 20;

    /// <summary>
    /// Preflight before a scan: is there room for <paramref name="expectedEntries"/> records at the planning
    /// estimate, plus the reserve? The runtime check before every spill remains authoritative.
    /// </summary>
    public (bool Sufficient, long EstimatedBytes, long Available) Preflight(long expectedEntries)
    {
        long estimate = checked(expectedEntries * EstimatedBytesPerRecord);
        long available = AvailableBytes();
        return (available - estimate >= ReserveBytes, estimate, available);
    }
}

/// <summary>Why hard links were not reconciled.</summary>
public enum ReconciliationFailure
{
    None,

    /// <summary>The caller did not request reconciliation.</summary>
    NotRequested,

    /// <summary>The temp-space preflight or a pre-spill check found too little free space.</summary>
    InsufficientTempSpace,

    /// <summary>A run could not be written or read back intact.</summary>
    RunStoreError,
}

/// <summary>
/// Collects <see cref="LinkRecord"/>s with bounded memory: records are buffered, sorted by file ID and
/// spilled to the run store whenever the buffer fills. <see cref="EnumerateGroups"/> merges the runs.
/// If a space check fails or a write throws, the writer is abandoned: runs are deleted, further records
/// are counted but discarded, and <see cref="Failure"/> says why.
/// </summary>
public sealed class HardLinkRunWriter
{
    private readonly IRunStore _store;
    private readonly TempSpaceGuard? _guard;
    private LinkRecord[]? _buffer;
    private int _count;
    private bool _finished;

    public HardLinkRunWriter(IRunStore store, int bufferCapacity = 256 * 1024, TempSpaceGuard? guard = null)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentOutOfRangeException.ThrowIfLessThan(bufferCapacity, 1);
        _store = store;
        _guard = guard;
        _buffer = new LinkRecord[bufferCapacity];
    }

    public long RecordsWritten { get; private set; }

    public long RecordsDiscarded { get; private set; }

    public int RunCount { get; private set; }

    public long BytesWritten => _store.BytesWritten;

    public ReconciliationFailure Failure { get; private set; }

    public string? FailureDetail { get; private set; }

    public bool IsAbandoned => Failure != ReconciliationFailure.None;

    public void Add(in LinkRecord record)
    {
        if (_finished)
        {
            throw new InvalidOperationException("The writer has been finished.");
        }

        if (_buffer is null)
        {
            RecordsDiscarded++;
            return;
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
            _buffer = null;
        }
    }

    /// <summary>Abandons reconciliation, deleting any runs written so far.</summary>
    public void Abandon(ReconciliationFailure reason, string detail)
    {
        if (reason == ReconciliationFailure.None)
        {
            throw new ArgumentOutOfRangeException(nameof(reason));
        }

        if (Failure == ReconciliationFailure.None)
        {
            Failure = reason;
            FailureDetail = detail;
        }

        RecordsDiscarded += _count;
        _count = 0;
        _buffer = null;
        _store.Clear();
    }

    /// <summary>
    /// Yields every file ID's records in ascending file-ID order. The yielded list is reused between
    /// iterations; callers must not keep it. Throws <see cref="InvalidDataException"/> if a run is damaged.
    /// </summary>
    public IEnumerable<IReadOnlyList<LinkRecord>> EnumerateGroups()
    {
        if (!_finished)
        {
            throw new InvalidOperationException("Call Finish before enumerating groups.");
        }

        if (IsAbandoned)
        {
            throw new InvalidOperationException($"Reconciliation was abandoned: {Failure}.");
        }

        IReadOnlyList<Stream> runs = _store.OpenRunsForReading();
        var readers = new LinkRunFormat.Reader[runs.Count];
        try
        {
            var queue = new PriorityQueue<int, LinkRecord>(Comparer<LinkRecord>.Create(LinkRecord.Compare));
            for (int i = 0; i < runs.Count; i++)
            {
                readers[i] = new LinkRunFormat.Reader(runs[i]);
                if (readers[i].TryRead(out LinkRecord first))
                {
                    queue.Enqueue(i, first);
                }
            }

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
            foreach (LinkRunFormat.Reader? reader in readers)
            {
                reader?.Dispose();
            }
        }
    }

    private void Spill()
    {
        if (_buffer is null || _count == 0)
        {
            return;
        }

        long worstCase = LinkRunFormat.HeaderBytes + LinkRunFormat.TrailerBytes + ((long)_count * LinkRunFormat.MaxRecordBytes);
        if (_guard is not null)
        {
            long available = _guard.AvailableBytes();
            if (available - worstCase < _guard.ReserveBytes)
            {
                Abandon(
                    ReconciliationFailure.InsufficientTempSpace,
                    $"{available} bytes free; a run of {_count} records may need up to {worstCase} bytes and {_guard.ReserveBytes} bytes must stay free.");
                return;
            }
        }

        Array.Sort(_buffer, 0, _count, Comparer<LinkRecord>.Create(LinkRecord.Compare));
        try
        {
            Stream stream = _store.CreateRun();
            LinkRunFormat.Write(stream, _buffer.AsSpan(0, _count));
            stream.Flush();
        }
        catch (IOException e)
        {
            Abandon(ReconciliationFailure.RunStoreError, e.Message);
            return;
        }

        Array.Clear(_buffer, 0, _count);
        _count = 0;
        RunCount++;
    }
}
