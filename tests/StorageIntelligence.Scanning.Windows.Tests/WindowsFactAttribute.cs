namespace StorageIntelligence.Scanning.Windows.Tests;

/// <summary>A fact that runs only on Windows; elsewhere it is reported as skipped, never as passed.</summary>
public sealed class WindowsFactAttribute : FactAttribute
{
    public WindowsFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            Skip = "Requires Windows.";
        }
    }
}
