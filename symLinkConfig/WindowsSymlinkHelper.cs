// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Win32.SafeHandles;
using System;
using System.Buffers;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace symLinkConfig
{
    internal static class WindowsSymLinkHelper
    {
        public static bool TryGetSymLinkTarget(string path, out string target)
        {
            target = null;

            while (IsSymbolicLink(path))
            {
                // Follow link so long as we are still finding symlinks
                target = GetSingleSymbolicLinkTarget(path);
                path = target;
            }

            return target != null;
        }
        internal static bool IsSymbolicLink(string path)
        {
            Interop.Kernel32.WIN32_FIND_DATA findData = new Interop.Kernel32.WIN32_FIND_DATA();
            using (SafeFindHandle handle = Interop.Kernel32.FindFirstFile(path, ref findData))
            {
                if (!handle.IsInvalid)
                {
                    return ((FileAttributes)findData.dwFileAttributes & FileAttributes.ReparsePoint) != 0 &&
                        (findData.dwReserved0 & 0xA000000C) != 0;  // IO_REPARSE_TAG_SYMLINK
                }
            }

            return false;
        }

        internal static string GetSymbolicLinkTarget(string path)
        {
            // Follow link so long as we are still finding symlinks
            do
            {
                path = GetSingleSymbolicLinkTarget(path);
            }
            while (IsSymbolicLink(path));

            return path;
        }

        internal static string GetSingleSymbolicLinkTarget(string path)
        {
            using (SafeFileHandle handle =
                Interop.Kernel32.CreateFile(path,
                0,                                                             // No file access required, this avoids file in use
                FileShare.ReadWrite | FileShare.Delete,                        // Share all access
                FileMode.Open,
                Interop.Kernel32.FileOperations.FILE_FLAG_OPEN_REPARSE_POINT | // Open the reparse point, not its target
                Interop.Kernel32.FileOperations.FILE_FLAG_BACKUP_SEMANTICS))   // Permit opening of directories
            {
                // https://docs.microsoft.com/en-us/windows-hardware/drivers/ifs/fsctl-get-reparse-point

                Interop.Kernel32.REPARSE_DATA_BUFFER_SYMLINK header;
                int sizeHeader = Marshal.SizeOf<Interop.Kernel32.REPARSE_DATA_BUFFER_SYMLINK>();
                uint bytesRead = 0;
                ReadOnlySpan<byte> validBuffer;
                int bufferSize = sizeHeader + Interop.Kernel32.MAX_PATH;

                while (true)
                {
                    byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                    try
                    {
                        int result = Interop.Kernel32.DeviceIoControl(handle, Interop.Kernel32.FSCTL_GET_REPARSE_POINT, null, 0, buffer, (uint)buffer.Length, out bytesRead, IntPtr.Zero) ?
                            0 : Marshal.GetLastWin32Error();

                        if (result != Interop.Errors.ERROR_SUCCESS && result != Interop.Errors.ERROR_INSUFFICIENT_BUFFER && result != Interop.Errors.ERROR_MORE_DATA)
                        {
                            throw new Win32Exception(result);
                        }

                        validBuffer = buffer.AsSpan().Slice(0, (int)bytesRead);

                        if (!MemoryMarshal.TryRead(validBuffer, out header))
                        {
                            if (result == Interop.Errors.ERROR_SUCCESS)
                            {
                                // didn't read enough for header
                                throw new InvalidDataException("FSCTL_GET_REPARSE_POINT did not return sufficient data");
                            }

                            // can't read header, guess at buffer length
                            buffer = new byte[buffer.Length + Interop.Kernel32.MAX_PATH];
                            continue;
                        }

                        // we only care about SubstituteName
                        bufferSize = sizeHeader + header.SubstituteNameOffset + header.SubstituteNameLength;

                        if (bytesRead >= bufferSize)
                        {
                            // got entire payload with valid header.
                            string target = Encoding.Unicode.GetString(validBuffer.Slice(sizeHeader + header.SubstituteNameOffset, header.SubstituteNameLength));

                            if ((header.Flags & Interop.Kernel32.SYMLINK_FLAG_RELATIVE) != 0)
                            {
                                if (PathInternal.IsExtended(path))
                                {
                                    target = path.Substring(0, 4) +
                                        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path.Substring(4)), target));
                                }
                                else
                                {
                                    target = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), target));
                                }
                            }

                            return target;
                        }

                        if (bufferSize < buffer.Length)
                        {
                            throw new InvalidDataException($"FSCTL_GET_REPARSE_POINT did not return sufficient data ({bufferSize}) when provided buffer ({buffer.Length}).");
                        }
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                }
            }
        }
    }
}
