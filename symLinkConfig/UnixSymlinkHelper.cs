// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;

#if USE_MONO_POSIX
using Mono.Unix;
#endif

namespace symLinkConfig
{
    internal static class UnixSymlinkHelper
    {
        internal static bool TryGetSymLinkTarget(string path, out string target, int maximumSymlinkDepth = 32)
        {
            target = null;
            int depth = 0;

#if USE_MONO_POSIX
            var symbolicLinkInfo = new UnixSymbolicLinkInfo(path);
            while (symbolicLinkInfo.Exists && symbolicLinkInfo.IsSymbolicLink)
            {
                target = symbolicLinkInfo.ContentsPath;

                if (!Path.IsPathFullyQualified(target))
                {
                    target = Path.GetFullPath(target, Path.GetDirectoryName(symbolicLinkInfo.FullName));
                }

                symbolicLinkInfo = new UnixSymbolicLinkInfo(target);

                if (depth++ > maximumSymlinkDepth)
                {
                    throw new InvalidOperationException("Exceeded maximum symlink depth");
                }
            }
#else
            // .NET Core System.Native shim implementation
            // these use the System.Native library which is internal to .NETCore and may change version-to-version.
            // Be sure to test this on your target framework, or use the Mono.Posix implementation instead
            while (IsSymbolicLink(path))
            {
                target = GetSymbolicLinkTarget(path);

                if (!Path.IsPathFullyQualified(target))
                {
                    target = Path.GetFullPath(target, Path.GetDirectoryName(path));
                }

                path = target;

                if (depth++ > maximumSymlinkDepth)
                {
                    throw new InvalidOperationException("Exceeded maximum symlink depth");
                }
            }
            
            bool IsSymbolicLink(string path)
            {
                Interop.Sys.FileStatus fileStatus;

                if (Interop.Sys.Stat(path, out fileStatus) < 0)
                {
                    return false;
                }

                return (fileStatus.Mode & Interop.Sys.FileTypes.S_IFLNK) == Interop.Sys.FileTypes.S_IFLNK;
            }

            string GetSymbolicLinkTarget(string path)
            {
                return Interop.Sys.ReadLink(path);
            }
#endif
            return target != null;
        }

    }
}
