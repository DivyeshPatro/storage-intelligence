using StorageIntelligence.Validation.Interop;

namespace StorageIntelligence.Validation;

/// <summary>
/// The only gate through which the validation tool may write. Fixture generation and the allocation harness call
/// <see cref="AssertWritable"/> first. A root is writable only if it is:
/// <list type="number">
/// <item>a directory under the user's temp folder named <c>si-test-*</c> that contains the marker file this guard
/// created when it made the directory, or</item>
/// <item>the root of a disposable CI volume: volume label starting with "SI", a <c>.si-disposable-volume</c> marker
/// at the volume root, and running under GitHub Actions with SI_ALLOW_TEST_VOLUMES=1.</item>
/// </list>
/// Everything else (including any real user folder) is refused.
/// </summary>
public static class TestRootGuard
{
    public const string TestRootMarker = ".si-test-root";
    public const string VolumeMarker = ".si-disposable-volume";

    /// <summary>Creates a fresh, marked test root under the temp folder.</summary>
    public static string CreateTempTestRoot()
    {
        string root = Path.Combine(Path.GetTempPath(), "si-test-" + Guid.NewGuid().ToString("N")[..12]);
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, TestRootMarker), "Created by StorageIntelligence.Validation. Safe to delete.");
        return root;
    }

    public static void AssertWritable(string root)
    {
        if (!IsWritable(root, out string reason))
        {
            throw new InvalidOperationException($"Refusing to write to '{root}': {reason}");
        }
    }

    public static bool IsWritable(string root, out string reason)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);
        string full = Path.GetFullPath(root).TrimEnd('\\');
        string temp = Path.GetFullPath(Path.GetTempPath()).TrimEnd('\\');

        if (full.StartsWith(temp + "\\", StringComparison.OrdinalIgnoreCase) &&
            Path.GetFileName(full).StartsWith("si-test-", StringComparison.Ordinal) &&
            File.Exists(Path.Combine(full, TestRootMarker)))
        {
            reason = "marked temp test root";
            return true;
        }

        string? volumeRoot = Path.GetPathRoot(full);
        if (volumeRoot is not null && string.Equals(full + "\\", volumeRoot, StringComparison.OrdinalIgnoreCase))
        {
            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") != "true" ||
                Environment.GetEnvironmentVariable("SI_ALLOW_TEST_VOLUMES") != "1")
            {
                reason = "volume roots are writable only on CI with SI_ALLOW_TEST_VOLUMES=1";
                return false;
            }

            if (!File.Exists(Path.Combine(volumeRoot, VolumeMarker)))
            {
                reason = $"volume has no {VolumeMarker} marker";
                return false;
            }

            string label = VolumeLabel(volumeRoot);
            if (!label.StartsWith("SI", StringComparison.Ordinal))
            {
                reason = $"volume label '{label}' does not start with SI";
                return false;
            }

            reason = "disposable CI volume";
            return true;
        }

        reason = "not a marked temp test root or a disposable CI volume root";
        return false;
    }

    private static unsafe string VolumeLabel(string volumeRoot)
    {
        char* label = stackalloc char[261];
        char* fs = stackalloc char[261];
        return ValidationNative.GetVolumeInformation(volumeRoot, label, 261, out _, out _, out _, fs, 261)
            ? new string(label)
            : "";
    }
}
