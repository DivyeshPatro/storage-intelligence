namespace StorageIntelligence.Accounting;

/// <summary>
/// Interns normalized file extensions to small integers so per-file work does not keep strings alive.
/// Bounded: once <see cref="Capacity"/> distinct extensions exist, new ones map to <see cref="Other"/>.
/// </summary>
public sealed class ExtensionTable
{
    public const string None = "(none)";
    public const string Other = "(other)";
    public const int MaxExtensionLength = 32;

    private readonly Dictionary<string, int> _ids = new(StringComparer.Ordinal);
    private readonly List<string> _names = [];

    public ExtensionTable(int capacity = 4096)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 2);
        Capacity = capacity;
        Intern(None);
        Intern(Other);
    }

    public int Capacity { get; }

    public int Count => _names.Count;

    public string NameOf(int id) => _names[id];

    /// <summary>Returns the id for the extension of <paramref name="fileName"/>.</summary>
    public int IdFor(string fileName) => Intern(Normalize(fileName));

    /// <summary>
    /// Lower-cased extension including the dot, following Windows rules: the text after the last dot,
    /// so ".gitignore" has extension ".gitignore" and "name." has none.
    /// </summary>
    public static string Normalize(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        int dot = fileName.LastIndexOf('.');
        if (dot < 0 || dot == fileName.Length - 1)
        {
            return None;
        }

        int length = fileName.Length - dot;
        if (length > MaxExtensionLength)
        {
            return Other;
        }

        return fileName.Substring(dot).ToLowerInvariant();
    }

    private int Intern(string normalized)
    {
        if (_ids.TryGetValue(normalized, out int id))
        {
            return id;
        }

        if (_names.Count >= Capacity)
        {
            return _ids[Other];
        }

        id = _names.Count;
        _names.Add(normalized);
        _ids.Add(normalized, id);
        return id;
    }
}

/// <summary>Whole-volume totals for one extension.</summary>
/// <param name="FileEntries">Directory entries (names) with this extension.</param>
/// <param name="DistinctFiles">Distinct files, after reconciliation; a hard-linked file counts once, under its canonical name's extension.</param>
public readonly record struct ExtensionStats(long FileEntries, long DistinctFiles, SizeVector Size)
{
    public static ExtensionStats operator +(ExtensionStats a, ExtensionStats b) =>
        new(a.FileEntries + b.FileEntries, a.DistinctFiles + b.DistinctFiles, a.Size + b.Size);

    public static ExtensionStats Add(ExtensionStats left, ExtensionStats right) => left + right;
}
