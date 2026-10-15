using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace StorageIntelligence.Validation.Interop;

/// <summary>
/// Win32 declarations for the validation tool. Deliberately separate from the scanner's interop: the reference
/// scanner enumerates with FindFirstFileExW and measures each file through its own handle, so it shares no
/// listing code with the production scanner. Fixture-only write operations (sparse, compression, links) live here
/// and are never reachable from the scanner assembly.
/// </summary>
internal static unsafe partial class ValidationNative
{
    internal const uint FILE_READ_ATTRIBUTES = 0x0080;
    internal const uint SYNCHRONIZE = 0x00100000;
    internal const uint FILE_SHARE_ALL = 0x7;
    internal const uint OPEN_EXISTING = 3;
    internal const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
    internal const uint FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;

    internal const int FileStandardInfo = 1;
    internal const int FileAttributeTagInfo = 9;
    internal const int FileIdInfo = 18;

    internal const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;
    internal const uint FILE_ATTRIBUTE_SPARSE_FILE = 0x200;
    internal const uint FILE_ATTRIBUTE_REPARSE_POINT = 0x400;
    internal const uint FILE_ATTRIBUTE_COMPRESSED = 0x800;

    internal const int ERROR_FILE_NOT_FOUND = 2;
    internal const int ERROR_NO_MORE_FILES = 18;

    internal const uint FSCTL_SET_SPARSE = 0x000900C4;
    internal const uint FSCTL_SET_COMPRESSION = 0x0009C040;
    internal const ushort COMPRESSION_FORMAT_DEFAULT = 1;

    internal const int FindExInfoBasic = 1;
    internal const int FindExSearchNameMatch = 0;
    internal const int FIND_FIRST_EX_LARGE_FETCH = 2;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct WIN32_FIND_DATAW
    {
        public uint dwFileAttributes;
        public long ftCreationTime;
        public long ftLastAccessTime;
        public long ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;
        public fixed char cFileName[260];
        public fixed char cAlternateFileName[14];
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FILE_STANDARD_INFO
    {
        public long AllocationSize;
        public long EndOfFile;
        public uint NumberOfLinks;
        public byte DeletePending;
        public byte Directory;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FILE_ID_INFO
    {
        public ulong VolumeSerialNumber;
        public ulong FileIdLow;
        public ulong FileIdHigh;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FILE_ATTRIBUTE_TAG_INFO
    {
        public uint FileAttributes;
        public uint ReparseTag;
    }

    [LibraryImport("kernel32.dll", EntryPoint = "FindFirstFileExW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial nint FindFirstFileEx(string fileName, int infoLevel, WIN32_FIND_DATAW* findData, int searchOp, nint searchFilter, int additionalFlags);

    [LibraryImport("kernel32.dll", EntryPoint = "FindNextFileW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool FindNextFile(nint findFile, WIN32_FIND_DATAW* findData);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool FindClose(nint findFile);

    [LibraryImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial SafeFileHandle CreateFile(string fileName, uint access, uint share, nint sa, uint disposition, uint flags, nint template);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetFileInformationByHandleEx(SafeFileHandle file, int infoClass, void* buffer, uint bufferSize);

    [LibraryImport("kernel32.dll", EntryPoint = "GetCompressedFileSizeW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial uint GetCompressedFileSize(string fileName, out uint fileSizeHigh);

    [LibraryImport("kernel32.dll", EntryPoint = "GetDiskFreeSpaceExW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetDiskFreeSpaceEx(string directory, out ulong freeToCaller, out ulong total, out ulong totalFree);

    [LibraryImport("kernel32.dll", EntryPoint = "CreateHardLinkW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CreateHardLink(string newLink, string existingFile, nint securityAttributes);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DeviceIoControl(SafeFileHandle device, uint code, void* inBuffer, uint inSize, void* outBuffer, uint outSize, out uint returned, nint overlapped);

    [LibraryImport("kernel32.dll", EntryPoint = "GetVolumeInformationW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetVolumeInformation(string root, char* label, uint labelSize, out uint serial, out uint maxComponent, out uint flags, char* fsName, uint fsNameSize);

    [LibraryImport("wofutil.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial int WofIsExternalFile(string filePath, out int isExternalFile, out uint provider, nint externalFileInfo, ref uint bufferLength);

    internal static string Extended(string path)
    {
        if (path.StartsWith(@"\\?\", StringComparison.Ordinal))
        {
            return path;
        }

        string full = Path.GetFullPath(path);
        return full.StartsWith(@"\\", StringComparison.Ordinal) ? @"\\?\UNC\" + full[2..] : @"\\?\" + full;
    }

    internal static SafeFileHandle OpenAttributes(string extendedPath) => CreateFile(
        extendedPath, FILE_READ_ATTRIBUTES | SYNCHRONIZE, FILE_SHARE_ALL, 0, OPEN_EXISTING,
        FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAG_OPEN_REPARSE_POINT, 0);

    internal static long CompressedSize(string extendedPath)
    {
        uint low = GetCompressedFileSize(extendedPath, out uint high);
        if (low == uint.MaxValue && Marshal.GetLastPInvokeError() != 0)
        {
            return -1;
        }

        return ((long)high << 32) | low;
    }

    internal static long TotalFreeBytes(string directory) =>
        GetDiskFreeSpaceEx(directory, out _, out _, out ulong totalFree)
            ? (long)totalFree
            : throw new IOException($"GetDiskFreeSpaceEx failed for {directory} ({Marshal.GetLastPInvokeError()}).");

    /// <summary>Returns (isExternal, provider) from the WOF API, or (false, 0) if WOF is unavailable.</summary>
    internal static (bool External, uint Provider) WofState(string path)
    {
        try
        {
            uint length = 0;
            int hr = WofIsExternalFile(path, out int external, out uint provider, 0, ref length);
            return hr >= 0 ? (external != 0, provider) : (false, 0);
        }
        catch (DllNotFoundException)
        {
            return (false, 0);
        }
    }
}
