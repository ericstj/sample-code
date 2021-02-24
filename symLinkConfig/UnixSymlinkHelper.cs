// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace symLinkConfig
{
    internal static class UnixSymlinkHelper
    {
        internal static bool TryGetSymLinkTarget(string path, out string target)
        {
            target = null;

            //Mono.Posix implementation 
            //To use add the following to the project:
            //    <PackageReference Include="Mono.Posix.NETStandard" Version="1.0.0" />

            //Mono.Unix.UnixSymbolicLinkInfo symbolicLinkInfo = new Mono.Unix.UnixSymbolicLinkInfo(path);

            //if (symbolicLinkInfo.IsSymbolicLink)
            //{
            //    target = symbolicLinkInfo.ContentsPath;
            //    return true;
            //}

            // .NET Core System.Native shim implementation
            // these use the System.Native library which is internal to .NETCore and may change version-to-version.
            // Be sure to test this on your target framework, or use the Mono.Posix implementation instead

            if (IsSymbolicLink(path))
            {
                target = GetSymbolicLinkTarget(path);
                return true;
            }

            return false;
        }

        private static bool IsSymbolicLink(string path)
        {
            Interop.Sys.FileStatus fileStatus;

            if (Interop.Sys.Stat(path, out fileStatus) < 0)
            {
                return false;
            }

            return (fileStatus.Mode & Interop.Sys.FileTypes.S_IFLNK) == Interop.Sys.FileTypes.S_IFLNK;
        }

        private static string GetSymbolicLinkTarget(string path)
        {
            return Interop.Sys.ReadLink(path);
        }

    }
}
