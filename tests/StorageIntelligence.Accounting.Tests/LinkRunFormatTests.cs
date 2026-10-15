using CsCheck;
using Xunit.Abstractions;
using StorageIntelligence.Accounting;
using StorageIntelligence.Accounting.Tests.Support;

namespace StorageIntelligence.Accounting.Tests;

public sealed class LinkRunFormatTests(ITestOutputHelper output)
{
    private static readonly Gen<LinkRecord> Record =
        Gen.Select(
            Gen.Select(Gen.Frequency((3, Gen.Const(0UL)), (1, Gen.ULong)), Gen.ULong),
            Gen.Long[0, long.MaxValue],
            Gen.Select(Gen.Long[0, long.MaxValue], Gen.Long[0, long.MaxValue]),
            Gen.UInt,
            Gen.Int[0, int.MaxValue],
            Gen.UInt,
            (id, dir, sizes, caveats, ext, links) => new LinkRecord(
                new FileId128(id.Item1, id.Item2), new DirectoryId(dir), sizes.Item1, sizes.Item2, (MeasurementCaveats)caveats, ext, links));

    [Fact]
    public void Round_trips_any_sorted_records() =>
        Record.Array[0, 200].Sample(records =>
        {
            Array.Sort(records, LinkRecord.Compare);
            using var ms = new MemoryStream();
            LinkRunFormat.Write(ms, records);
            ms.Position = 0;
            using var reader = new LinkRunFormat.Reader(ms);
            var back = new List<LinkRecord>();
            while (reader.TryRead(out LinkRecord r))
            {
                back.Add(r);
            }

            Assert.Equal(records, back);
            Assert.False(reader.TryRead(out _));
        }, iter: 2000);

    [Fact]
    public void Rejects_unknown_version_bad_magic_truncation_and_bad_trailer()
    {
        byte[] good = Encode([Sample(1), Sample(2), Sample(3)]);

        byte[] version = (byte[])good.Clone();
        version[4] = 2;
        Assert.Contains("version 2", Assert.Throws<InvalidDataException>(() => ReadAll(version)).Message);

        byte[] magic = (byte[])good.Clone();
        magic[0] = (byte)'X';
        Assert.Throws<InvalidDataException>(() => ReadAll(magic));

        Assert.Throws<InvalidDataException>(() => ReadAll(good[..^1]));      // trailer cut short
        Assert.Throws<InvalidDataException>(() => ReadAll(good[..^12]));     // records cut short

        byte[] trailer = (byte[])good.Clone();
        trailer[^1] ^= 0x7F;
        Assert.Throws<InvalidDataException>(() => ReadAll(trailer));
    }

    [Fact]
    public void Encoding_is_compact_for_ntfs_like_identities()
    {
        // NTFS 64-bit file IDs: 48-bit MFT segment number + 16-bit sequence number in the top bits.
        var random = new Random(42);
        var records = new LinkRecord[100_000];
        for (int i = 0; i < records.Length; i++)
        {
            ulong sequence = (ulong)random.Next(1, 30);
            ulong segment = (ulong)(i * 3 + random.Next(3));
            long logical = (long)(Math.Exp(random.NextDouble() * 20));
            records[i] = new LinkRecord(
                new FileId128(0, (sequence << 48) | segment),
                new DirectoryId(random.Next(0, 200_000)),
                logical,
                (logical + 4095) / 4096 * 4096,
                MeasurementCaveats.UnvalidatedFileSystem,
                random.Next(0, 300),
                0);
        }

        Array.Sort(records, LinkRecord.Compare);
        byte[] encoded = Encode(records);
        double bytesPerRecord = (double)encoded.Length / records.Length;
        output.WriteLine($"v1 encoding: {bytesPerRecord:F2} bytes/record over {records.Length} NTFS-like records (v0 fixed width: 52)");
        Assert.InRange(bytesPerRecord, 1, 20); // fixed-width v0 used 52 bytes per record
    }

    [Fact]
    public void Insufficient_temp_space_abandons_reconciliation_with_an_explicit_state()
    {
        long free = 10_000;
        var store = new InMemoryRunStore();
        var writer = new HardLinkRunWriter(store, bufferCapacity: 4, new TempSpaceGuard(() => free, ReserveBytes: 9_900));
        var sink = new InMemoryScanStore();
        var policy = new MeasurementPolicy { FileSystem = FileSystemKind.Ntfs, FileSystemValidated = true };
        var agg = new ScanAggregator(new AggregatorOptions { Policy = policy }, sink, writer);
        DirectoryId root = agg.BeginRoot("C:");
        for (int i = 0; i < 10; i++)
        {
            agg.AddFile(root, new FileObservation($"f{i}.bin", TreeModel.IdFor(i % 3), 4096, 4096));
        }

        agg.EndListing(root);
        ScanAccounting a = ScanAccounting.Build(agg.Complete(), sink, writer, policy);

        Assert.Equal(HardLinkStatus.NotReconciled, a.HardLinks);
        Assert.Equal(ReconciliationFailure.InsufficientTempSpace, a.ReconciliationFailure);
        Assert.NotNull(a.ReconciliationFailureDetail);
        Assert.Equal(0, store.BytesWritten); // runs were deleted
        Assert.Equal(10 * 4096, a.Root.Size.Allocated); // streaming upper bound, explicitly labelled
        Assert.False(a.Root.IsComplete);
        Assert.True(writer.RecordsDiscarded > 0);
    }

    [Fact]
    public void Preflight_reports_estimate_and_sufficiency()
    {
        var guard = new TempSpaceGuard(() => 1_000_000, ReserveBytes: 500_000);
        Assert.True(guard.Preflight(10_000).Sufficient);   // 200 kB estimate
        Assert.False(guard.Preflight(100_000).Sufficient); // 2 MB estimate
    }

    [Fact]
    public void A_damaged_run_yields_not_reconciled_instead_of_wrong_totals()
    {
        var store = new CorruptingRunStore();
        var writer = new HardLinkRunWriter(store, bufferCapacity: 2);
        var sink = new InMemoryScanStore();
        var policy = new MeasurementPolicy { FileSystem = FileSystemKind.Ntfs, FileSystemValidated = true };
        var agg = new ScanAggregator(new AggregatorOptions { Policy = policy }, sink, writer);
        DirectoryId root = agg.BeginRoot("C:");
        agg.AddFile(root, new FileObservation("a", TreeModel.IdFor(1), 1, 4096));
        agg.AddFile(root, new FileObservation("b", TreeModel.IdFor(1), 1, 4096));
        agg.EndListing(root);
        ScanAccounting a = ScanAccounting.Build(agg.Complete(), sink, writer, policy);

        Assert.Equal(HardLinkStatus.NotReconciled, a.HardLinks);
        Assert.Equal(ReconciliationFailure.RunStoreError, a.ReconciliationFailure);
        Assert.Null(a.Reconciliation);
    }

    private static LinkRecord Sample(ulong id) =>
        new(new FileId128(0, id), new DirectoryId((long)id), 100, 4096, MeasurementCaveats.None, 1, 0);

    private static byte[] Encode(LinkRecord[] records)
    {
        using var ms = new MemoryStream();
        LinkRunFormat.Write(ms, records);
        return ms.ToArray();
    }

    private static List<LinkRecord> ReadAll(byte[] bytes)
    {
        using var reader = new LinkRunFormat.Reader(new MemoryStream(bytes));
        var list = new List<LinkRecord>();
        while (reader.TryRead(out LinkRecord r))
        {
            list.Add(r);
        }

        return list;
    }

    /// <summary>Flips the last byte of every run when it is read back, simulating on-disk damage.</summary>
    private sealed class CorruptingRunStore : IRunStore
    {
        private readonly InMemoryRunStore _inner = new();

        public long BytesWritten => _inner.BytesWritten;

        public Stream CreateRun() => _inner.CreateRun();

        public IReadOnlyList<Stream> OpenRunsForReading()
        {
            var runs = _inner.OpenRunsForReading();
            foreach (Stream run in runs)
            {
                run.Position = run.Length - 1;
                int b = run.ReadByte();
                run.Position = run.Length - 1;
                run.WriteByte((byte)(b ^ 0xFF));
                run.Position = 0;
            }

            return runs;
        }

        public void Clear() => _inner.Clear();

        public void Dispose() => _inner.Dispose();
    }
}
