using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace symLinkConfig
{
    public static class SymlinkHelper
    {
        private static bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsSymbolicLink(string path) => isWindows ? WindowsSymLinkHelper.IsSymbolicLink(path) : UnixSymlinkHelper.IsSymbolicLink(path);
        public static bool TryGetSymLinkTarget(string path, out string target, int maximumSymlinkDepth = 32) => isWindows ? WindowsSymLinkHelper.TryGetSymLinkTarget(path, out target, maximumSymlinkDepth) : UnixSymlinkHelper.TryGetSymLinkTarget(path, out target, maximumSymlinkDepth);
        public static DateTime GetSymbolicLinkTargetLastWriteTime(string path) => isWindows ? WindowsSymLinkHelper.GetSymbolicLinkTargetLastWriteTime(path) : UnixSymlinkHelper.GetSymbolicLinkTargetLastWriteTime(path);
    }
}
