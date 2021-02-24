// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetFinalPathNameByHandleW(SafeFileHandle hFile, StringBuilder lpszFilePath, int cchFilePath, int dwFlags);

        internal static string GetFinalPathNameByHandle(SafeFileHandle hFile)
        {
            StringBuilder builder = new StringBuilder(260);

            int result = GetFinalPathNameByHandleW(hFile, builder, builder.Capacity, 0);
            if (result == 0)
            {
                throw new Win32Exception();
            }

            while (result > builder.Capacity)
            {
                builder.Capacity = result;

                result = GetFinalPathNameByHandleW(hFile, builder, builder.Capacity, 0);

                if (result == 0)
                {
                    throw new Win32Exception();
                }

                if (result > builder.Capacity)
                {
                    // ignore inconsistent responses 
                    return null;
                }
            }

            return builder.ToString();
        }
    }
}
