using System.Runtime.InteropServices;
using StorageIntelligence.Validation.Interop;

namespace StorageIntelligence.Scanning.Windows.Tests;

/// <summary>Struct layouts must match the Win32 definitions exactly; a wrong offset silently corrupts every field.</summary>
public sealed class InteropLayoutTests
{
    [Fact]
    public void Win32_find_data_matches_the_documented_layout()
    {
        // WIN32_FIND_DATAW: attributes 0, three FILETIMEs 4/12/20, sizes 28/32, reserved 36/40, cFileName 44, total 592.
        Assert.Equal(4, (int)Marshal.OffsetOf<ValidationNative.WIN32_FIND_DATAW>(nameof(ValidationNative.WIN32_FIND_DATAW.ftCreationTime)));
        Assert.Equal(28, (int)Marshal.OffsetOf<ValidationNative.WIN32_FIND_DATAW>(nameof(ValidationNative.WIN32_FIND_DATAW.nFileSizeHigh)));
        Assert.Equal(36, (int)Marshal.OffsetOf<ValidationNative.WIN32_FIND_DATAW>(nameof(ValidationNative.WIN32_FIND_DATAW.dwReserved0)));
        Assert.Equal(44, (int)Marshal.OffsetOf<ValidationNative.WIN32_FIND_DATAW>(nameof(ValidationNative.WIN32_FIND_DATAW.cFileName)));
        Assert.Equal(592, Marshal.SizeOf<ValidationNative.WIN32_FIND_DATAW>());
    }

    [Fact]
    public void Handle_information_structs_match_the_documented_sizes()
    {
        Assert.Equal(24, Marshal.SizeOf<ValidationNative.FILE_STANDARD_INFO>());
        Assert.Equal(24, Marshal.SizeOf<ValidationNative.FILE_ID_INFO>());
        Assert.Equal(8, Marshal.SizeOf<ValidationNative.FILE_ATTRIBUTE_TAG_INFO>());
        Assert.Equal(24, Marshal.SizeOf<StorageIntelligence.Scanning.Windows.Interop.Native.FILE_STANDARD_INFO>());
        Assert.Equal(40, Marshal.SizeOf<StorageIntelligence.Scanning.Windows.Interop.Native.FILE_BASIC_INFO>());
    }
}
