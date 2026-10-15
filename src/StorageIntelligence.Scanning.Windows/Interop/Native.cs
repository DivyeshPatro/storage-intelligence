using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace StorageIntelligence.Scanning.Windows.Interop;

/// <summary>
/// Win32 declarations used by the scanner. Every handle the scanner opens is metadata-only:
/// <see cref="FILE_LIST_DIRECTORY"/> for directories and <see cref="FILE_READ_ATTRIBUTES"/> for files, always with
/// <see cref="FILE_FLAG_OPEN_REPARSE_POINT"/> so a reparse point is opened itself and never followed.
/// No handle with read-data, write or delete access is ever requested.
/// </summary>
internal static unsafe partial class Native
{
    internal const uint FILE_LIST_DIRECTORY = 0x0001;
    internal const uint FILE_READ_ATTRIBUTES = 0x0080;
    internal const uint SYNCHRONIZE = 0x00100000;

    internal const uint FILE_SHARE_ALL = 0x1 | 0x2 | 0x4;
    internal const uint OPEN_EXISTING = 3;
    internal const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
    internal const uint FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;

    internal const int ERROR_FILE_NOT_FOUND = 2;
    internal const int ERROR_PATH_NOT_FOUND = 3;
    internal const int ERROR_ACCESS_DENIED = 5;
    internal const int ERROR_INVALID_FUNCTION = 1;
    internal const int ERROR_NOT_SUPPORTED = 50;
    internal const int ERROR_INVALID_PARAMETER = 87;
    internal const int ERROR_NO_MORE_FILES = 18;
    internal const int ERROR_SHARING_VIOLATION = 32;

    internal const int FileBasicInfo = 0;
    internal const int FileStandardInfo = 1;
    internal const int FileAttributeTagInfo = 9;
    internal const int FileFullDirectoryInfo = 14;
    internal const int FileIdInfo = 18;
    internal const int FileIdExtdDirectoryInfo = 19;

    internal const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;
    internal const uint FILE_ATTRIBUTE_SPARSE_FILE = 0x200;
    internal const uint FILE_ATTRIBUTE_REPARSE_POINT = 0x400;
    internal const uint FILE_ATTRIBUTE_COMPRESSED = 0x800;
    internal const uint FILE_ATTRIBUTE_OFFLINE = 0x1000;
    internal const uint FILE_ATTRIBUTE_RECALL_ON_OPEN = 0x40000;
    internal const uint FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS = 0x400000;

    internal const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
    internal const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;
    internal const uint IO_REPARSE_TAG_WOF = 0x80000017;
    internal const uint IO_REPARSE_TAG_CLOUD_MASK = 0xFFFF0FFF;
    internal const uint IO_REPARSE_TAG_CLOUD = 0x9000001A;

    internal const uint FSCTL_GET_EXTERNAL_BACKING = 0x00090310;
    internal const int ERROR_OBJECT_NOT_EXTERNALLY_BACKED = 342;
    internal const int ERROR_INSUFFICIENT_BUFFER = 122;
    internal const int ERROR_MORE_DATA = 234;

    internal const int THREAD_MODE_BACKGROUND_BEGIN = 0x00010000;
    internal const int THREAD_MODE_BACKGROUND_END = 0x00020000;

    internal static bool IsNameSurrogate(uint tag) => (tag & 0x20000000) != 0;

    [StructLayout(LayoutKind.Sequential)]
    internal struct FILE_BASIC_INFO
    {
        public long CreationTime;
        public long LastAccessTime;
        public long LastWriteTime;
        public long ChangeTime;
        public uint FileAttributes;
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

    [LibraryImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial SafeFileHandle CreateFile(
        string fileName, uint desiredAccess, uint shareMode, nint securityAttributes, uint creationDisposition, uint flagsAndAttributes, nint templateFile);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetFileInformationByHandleEx(SafeFileHandle file, int infoClass, void* buffer, uint bufferSize);

    [LibraryImport("kernel32.dll", EntryPoint = "GetVolumeInformationByHandleW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetVolumeInformationByHandle(
        SafeFileHandle file, char* volumeName, uint volumeNameSize, out uint serialNumber, out uint maxComponentLength,
        out uint fileSystemFlags, char* fileSystemName, uint fileSystemNameSize);

    [LibraryImport("kernel32.dll", EntryPoint = "GetDiskFreeSpaceW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetDiskFreeSpace(
        string rootPathName, out uint sectorsPerCluster, out uint bytesPerSector, out uint freeClusters, out uint totalClusters);

    [LibraryImport("kernel32.dll", EntryPoint = "GetDiskFreeSpaceExW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetDiskFreeSpaceEx(
        string directoryName, out ulong freeBytesAvailableToCaller, out ulong totalBytes, out ulong totalFreeBytes);

    [LibraryImport("kernel32.dll", EntryPoint = "GetVolumePathNameW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetVolumePathName(string fileName, char* volumePathName, uint bufferLength);

    [LibraryImport("kernel32.dll", EntryPoint = "GetCompressedFileSizeW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial uint GetCompressedFileSize(string fileName, out uint fileSizeHigh);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DeviceIoControl(
        SafeFileHandle device, uint ioControlCode, void* inBuffer, uint inBufferSize, void* outBuffer, uint outBufferSize, out uint bytesReturned, nint overlapped);

    [LibraryImport("kernel32.dll")]
    internal static partial nint GetCurrentThread();

    /// <summary>
    /// True if the file is backed by an external provider such as WOF (FSCTL_GET_EXTERNAL_BACKING). This is a metadata
    /// query on a FILE_READ_ATTRIBUTES handle; no file content is read.
    /// </summary>
    internal static bool? IsExternallyBacked(SafeFileHandle handle)
    {
        byte* buffer = stackalloc byte[64];
        if (DeviceIoControl(handle, FSCTL_GET_EXTERNAL_BACKING, null, 0, buffer, 64, out _, 0))
        {
            return true;
        }

        return Marshal.GetLastPInvokeError() switch
        {
            ERROR_OBJECT_NOT_EXTERNALLY_BACKED => false,
            ERROR_INSUFFICIENT_BUFFER or ERROR_MORE_DATA => true,
            _ => null,
        };
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetThreadPriority(nint thread, int priority);

    /// <summary>Converts a path to the extended-length form so long paths and unusual names work.</summary>
    internal static string ToExtendedPath(string path)
    {
        if (path.StartsWith(@"\\?\", StringComparison.Ordinal))
        {
            return path;
        }

        string full = Path.GetFullPath(path);
        return full.StartsWith(@"\\", StringComparison.Ordinal) ? @"\\?\UNC\" + full[2..] : @"\\?\" + full;
    }

    /// <summary>Opens a file or directory for metadata only, without following reparse points.</summary>
    internal static SafeFileHandle OpenMetadata(string extendedPath, uint access, out int error)
    {
        SafeFileHandle handle = CreateFile(
            extendedPath,
            access | SYNCHRONIZE,
            FILE_SHARE_ALL,
            0,
            OPEN_EXISTING,
            FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAG_OPEN_REPARSE_POINT,
            0);
        error = handle.IsInvalid ? Marshal.GetLastPInvokeError() : 0;
        return handle;
    }

    internal static bool TryGetBasicInfo(SafeFileHandle handle, out FILE_BASIC_INFO info)
    {
        FILE_BASIC_INFO local;
        bool ok = GetFileInformationByHandleEx(handle, FileBasicInfo, &local, (uint)sizeof(FILE_BASIC_INFO));
        info = local;
        return ok;
    }

    internal static bool TryGetStandardInfo(SafeFileHandle handle, out FILE_STANDARD_INFO info)
    {
        FILE_STANDARD_INFO local;
        bool ok = GetFileInformationByHandleEx(handle, FileStandardInfo, &local, (uint)sizeof(FILE_STANDARD_INFO));
        info = local;
        return ok;
    }
}
