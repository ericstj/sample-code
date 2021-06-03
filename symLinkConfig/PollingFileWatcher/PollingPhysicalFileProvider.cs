// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.FileProviders.Physical.Internal;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.FileProviders
{
    /// <summary>
    /// Looks up files using the on-disk file system
    /// </summary>
    public class PollingPhysicalFileProvider : IFileProvider, IDisposable
    {
        private static readonly char[] _pathSeparators = new[]
            {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};

        private readonly ExclusionFilters _filters;
        private PollingPhysicalFilesWatcher _fileWatcher;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of a PollingPhysicalFileProvider at the given root directory.
        /// </summary>
        /// <param name="root">The root directory. This should be an absolute path.</param>
        public PollingPhysicalFileProvider(string root)
            : this(root, ExclusionFilters.Sensitive)
        {
        }

        /// <summary>
        /// Initializes a new instance of a PollingPhysicalFileProvider at the given root directory.
        /// </summary>
        /// <param name="root">The root directory. This should be an absolute path.</param>
        /// <param name="filters">Specifies which files or directories are excluded.</param>
        public PollingPhysicalFileProvider(string root, ExclusionFilters filters)
        {
            if (!Path.IsPathRooted(root))
            {
                throw new ArgumentException("The path must be absolute.", nameof(root));
            }

            string fullRoot = Path.GetFullPath(root);
            // When we do matches in GetFullPath, we want to only match full directory names.
            Root = PathUtils.EnsureTrailingSlash(fullRoot);
            if (!Directory.Exists(Root))
            {
                throw new DirectoryNotFoundException(Root);
            }

            _fileWatcher = new PollingPhysicalFilesWatcher(Root);
        }

        /// <summary>
        /// Disposes the provider. Change tokens may not trigger after the provider is disposed.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the provider.
        /// </summary>
        /// <param name="disposing"><c>true</c> is invoked from <see cref="IDisposable.Dispose"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _fileWatcher?.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// The root directory for this instance.
        /// </summary>
        public string Root { get; }

        private string GetFullPath(string path)
        {
            if (PathUtils.PathNavigatesAboveRoot(path))
            {
                return null;
            }

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(Path.Combine(Root, path));
            }
            catch
            {
                return null;
            }

            if (!IsUnderneathRoot(fullPath))
            {
                return null;
            }

            return fullPath;
        }

        private bool IsUnderneathRoot(string fullPath)
        {
            return fullPath.StartsWith(Root, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Locate a file at the given path by directly mapping path segments to physical directories.
        /// </summary>
        /// <param name="subpath">A path under the root directory</param>
        /// <returns>The file information. Caller must check <see cref="IFileInfo.Exists"/> property. </returns>
        public IFileInfo GetFileInfo(string subpath)
        {
            if (string.IsNullOrEmpty(subpath) || PathUtils.HasInvalidPathChars(subpath))
            {
                return new NotFoundFileInfo(subpath);
            }

            // Relative paths starting with leading slashes are okay
            subpath = subpath.TrimStart(_pathSeparators);

            // Absolute paths not permitted.
            if (Path.IsPathRooted(subpath))
            {
                return new NotFoundFileInfo(subpath);
            }

            string fullPath = GetFullPath(subpath);
            if (fullPath == null)
            {
                return new NotFoundFileInfo(subpath);
            }

            var fileInfo = new FileInfo(fullPath);
            if (FileSystemInfoHelper.IsExcluded(fileInfo, _filters))
            {
                return new NotFoundFileInfo(subpath);
            }

            return new PhysicalFileInfo(fileInfo);
        }

        /// <summary>
        /// Enumerate a directory at the given path, if any.
        /// </summary>
        /// <param name="subpath">A path under the root directory. Leading slashes are ignored.</param>
        /// <returns>
        /// Contents of the directory. Caller must check <see cref="IDirectoryContents.Exists"/> property. <see cref="NotFoundDirectoryContents" /> if
        /// <paramref name="subpath" /> is absolute, if the directory does not exist, or <paramref name="subpath" /> has invalid
        /// characters.
        /// </returns>
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            try
            {
                if (subpath == null || PathUtils.HasInvalidPathChars(subpath))
                {
                    return NotFoundDirectoryContents.Singleton;
                }

                // Relative paths starting with leading slashes are okay
                subpath = subpath.TrimStart(_pathSeparators);

                // Absolute paths not permitted.
                if (Path.IsPathRooted(subpath))
                {
                    return NotFoundDirectoryContents.Singleton;
                }

                string fullPath = GetFullPath(subpath);
                if (fullPath == null || !Directory.Exists(fullPath))
                {
                    return NotFoundDirectoryContents.Singleton;
                }

                return new PhysicalDirectoryContents(fullPath, _filters);
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (IOException)
            {
            }
            return NotFoundDirectoryContents.Singleton;
        }

        /// <summary>
        ///     <para>Creates a <see cref="IChangeToken" /> for the specified <paramref name="filter" />.</para>
        ///     <para>Globbing patterns are interpreted by <seealso cref="Microsoft.Extensions.FileSystemGlobbing.Matcher" />.</para>
        /// </summary>
        /// <param name="filter">
        /// Filter string used to determine what files or folders to monitor. Example: **/*.cs, *.*,
        /// subFolder/**/*.cshtml.
        /// </param>
        /// <returns>
        /// An <see cref="IChangeToken" /> that is notified when a file matching <paramref name="filter" /> is added,
        /// modified or deleted. Returns a <see cref="NullChangeToken" /> if <paramref name="filter" /> has invalid filter
        /// characters or if <paramref name="filter" /> is an absolute path or outside the root directory specified in the
        /// constructor <seealso cref="PhysicalFileProvider(string)" />.
        /// </returns>
        public IChangeToken Watch(string filter)
        {
            if (filter == null || PathUtils.HasInvalidFilterChars(filter))
            {
                return NullChangeToken.Singleton;
            }

            // Relative paths starting with leading slashes are okay
            filter = filter.TrimStart(_pathSeparators);

            return _fileWatcher.CreateFileChangeToken(filter);
        }
    }
}
