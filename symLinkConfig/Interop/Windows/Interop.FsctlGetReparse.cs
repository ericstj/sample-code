// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
        internal const uint FSCTL_GET_REPARSE_POINT = 0x000900a8;
        internal const uint SYMLINK_FLAG_RELATIVE = 1;
        internal struct REPARSE_DATA_BUFFER_SYMLINK
        {
            public uint ReparseTag;
            public ushort ReparseDataLength;
            public ushort Reserved;

            // SymbolicLinkReparseBuffer members, we only care about this part of union
            public ushort SubstituteNameOffset;
            public ushort SubstituteNameLength;
            public ushort PrintNameOffset;
            public ushort PrintNameLength;
            public uint Flags;
        }

        
        [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool DeviceIoControl
        (
            SafeFileHandle fileHandle,
            uint ioControlCode,
            [In] byte[] inBuffer,
            uint cbInBuffer,
            [Out] byte[] outBuffer,
            uint cbOutBuffer,
            out uint cbBytesReturned,
            IntPtr overlapped
        );
    }
}
