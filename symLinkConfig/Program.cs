// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
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

            //while (true)
            //{
            //    System.Threading.Thread.Sleep(1000);
            //    Console.WriteLine(WindowsSymLinkHelper.GetSymbolicLinkTargetLastWriteTime("config\\appsettings.json"));
            //}
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

            if (SymlinkHelper.IsSymbolicLink(fileInfo.PhysicalPath))
            {
                string targetDirectory = Path.GetDirectoryName(fileInfo.PhysicalPath);
                string targetFile = Path.GetFileName(fileInfo.PhysicalPath);
                c.AddJsonFile(new PollingPhysicalFileProvider(targetDirectory), targetFile, optional, reloadOnChange);
            }
            else
            {
                c.AddJsonFile(relativePath, optional, reloadOnChange);
            }
        }
    }
}
