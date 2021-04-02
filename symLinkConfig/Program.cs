// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace symLinkConfig
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration(c => c.AddLinkedJsonFile("config/appsettings.json",
                        optional: true, reloadOnChange: true));

    }

    internal static class ConfigurationExtensions
    {
        internal static void AddLinkedJsonFile(this IConfigurationBuilder c, string relativePath, bool optional, bool reloadOnChange)
        {
            var fileInfo = c.GetFileProvider().GetFileInfo(relativePath);

            if (TryGetSymLinkTarget(fileInfo.PhysicalPath, out string targetPath))
            {
                string targetDirectory = Path.GetDirectoryName(targetPath);
                string targetFile = Path.GetFileName(targetPath);

                if (TryGetSymLinkTarget(targetDirectory, out string symlinkDirectory))
                {
                    targetDirectory = symlinkDirectory;
                }

                Console.WriteLine($"Adding file {targetFile} from {targetDirectory}");

                c.AddJsonFile(new PhysicalFileProvider(targetDirectory), targetFile, optional, reloadOnChange);
            }
            else
            {
                c.AddJsonFile(relativePath, optional, reloadOnChange);
            }
        }

        private static bool TryGetSymLinkTarget(string path, out string target)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsSymLinkHelper.TryGetSymLinkTarget(path, out target);
            }
            else
            {
                return UnixSymlinkHelper.TryGetSymLinkTarget(path, out target);
            }
        }

    }
}
