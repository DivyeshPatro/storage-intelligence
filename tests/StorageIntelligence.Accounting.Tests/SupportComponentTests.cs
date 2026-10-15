using StorageIntelligence.Accounting;

namespace StorageIntelligence.Accounting.Tests;

public sealed class SupportComponentTests
{
    [Theory]
    [InlineData("a.TXT", ".txt")]
    [InlineData("archive.tar.gz", ".gz")]
    [InlineData(".gitignore", ".gitignore")]
    [InlineData("README", "(none)")]
    [InlineData("trailing.", "(none)")]
    [InlineData("x.thisextensioniswaytoolongtobeanextension", "(other)")]
    public void Extensions_follow_windows_rules(string name, string expected) =>
        Assert.Equal(expected, ExtensionTable.Normalize(name));

    [Fact]
    public void Extension_table_is_bounded()
    {
        var table = new ExtensionTable(capacity: 4);
        Assert.Equal(".a", table.NameOf(table.IdFor("x.a")));
        Assert.Equal(".b", table.NameOf(table.IdFor("x.b")));
        Assert.Equal(ExtensionTable.Other, table.NameOf(table.IdFor("x.c")));
        Assert.Equal(4, table.Count);
    }

    [Fact]
    public void Temp_file_runs_round_trip_and_are_deleted_once_merged()
    {
        string dir = Path.Combine(Path.GetTempPath(), "si-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            using (var store = new TempFileRunStore(dir))
            {
                var writer = new HardLinkRunWriter(store, bufferCapacity: 3);
                for (int i = 0; i < 10; i++)
                {
                    writer.Add(new LinkRecord(new FileId128(1, (ulong)(i % 4)), new DirectoryId(i), i, i * 4096L, MeasurementCaveats.Sparse, 2, 1));
                }

                writer.Finish();
                Assert.Equal(4, writer.RunCount);
                Assert.True(store.BytesWritten > 0);
                Assert.Equal(4, Directory.GetFiles(dir).Length);
                var groups = writer.EnumerateGroups().Select(g => g.ToList()).ToList();
                Assert.Equal(4, groups.Count);
                Assert.Equal(new LinkRecord(new FileId128(1, 1), new DirectoryId(5), 5, 5 * 4096L, MeasurementCaveats.Sparse, 2, 1), groups[1][1]);
                Assert.Empty(Directory.GetFiles(dir)); // each run is deleted as soon as it has been merged
            }

            Assert.Empty(Directory.GetFiles(dir));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
