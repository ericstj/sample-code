// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders.Physical.Internal;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.FileProviders.Physical
{
    /// <summary>
    ///     <para>
    ///     A file watcher that watches a physical filesystem for changes.
    ///     </para>
    ///     <para>
    ///     Triggers events on <see cref="IChangeToken" /> when files are created, change, renamed, or deleted.
    ///     </para>
    /// </summary>
    public class PollingPhysicalFilesWatcher : IDisposable
    {
        internal static TimeSpan DefaultPollingInterval = TimeSpan.FromSeconds(4);

        private readonly string _root;

        private Timer _timer;
        private bool _timerInitialzed;
        private object _timerLock = new object();
        private Func<Timer> _timerFactory;
        private bool _disposed;

        /// <summary>
        /// Initializes an instance of <see cref="PhysicalFilesWatcher" /> that watches files in <paramref name="root" />.
        /// Wraps an instance of <see cref="System.IO.FileSystemWatcher" />
        /// </summary>
        /// <param name="root">Root directory for the watcher</param>
        /// <param name="fileSystemWatcher">The wrapped watcher that is watching <paramref name="root" /></param>
        /// <param name="pollForChanges">
        /// True when the watcher should use polling to trigger instances of
        /// <see cref="IChangeToken" /> created by <see cref="CreateFileChangeToken(string)" />
        /// </param>
        public PollingPhysicalFilesWatcher(
            string root)
        {
            _root = root;

            PollingChangeTokens = new ConcurrentDictionary<IPollingChangeToken, IPollingChangeToken>();
            _timerFactory = () => NonCapturingTimer.Create(RaiseChangeEvents, state: PollingChangeTokens, dueTime: TimeSpan.Zero, period: DefaultPollingInterval);
        }

        internal ConcurrentDictionary<IPollingChangeToken, IPollingChangeToken> PollingChangeTokens { get; }

        /// <summary>
        ///     <para>
        ///     Creates an instance of <see cref="IChangeToken" /> for all files and directories that match the
        ///     <paramref name="filter" />
        ///     </para>
        ///     <para>
        ///     Globbing patterns are relative to the root directory given in the constructor
        ///     <seealso cref="PhysicalFilesWatcher(string, FileSystemWatcher, bool)" />. Globbing patterns
        ///     are interpreted by <seealso cref="Matcher" />.
        ///     </para>
        /// </summary>
        /// <param name="filter">A globbing pattern for files and directories to watch</param>
        /// <returns>A change token for all files that match the filter</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="filter" /> is null</exception>
        public IChangeToken CreateFileChangeToken(string filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            filter = NormalizePath(filter);

            // Absolute paths and paths traversing above root not permitted.
            if (Path.IsPathRooted(filter) || PathUtils.PathNavigatesAboveRoot(filter))
            {
                return NullChangeToken.Singleton;
            }

            IChangeToken changeToken = GetOrAddChangeToken(filter);

            return changeToken;
        }

        private IChangeToken GetOrAddChangeToken(string pattern)
        {
            LazyInitializer.EnsureInitialized(ref _timer, ref _timerInitialzed, ref _timerLock, _timerFactory);

            IPollingChangeToken pollingChangeToken;
            bool isWildCard = pattern.IndexOf('*') != -1;
            if (isWildCard || IsDirectoryPath(pattern))
            {
                pollingChangeToken = new PollingWildCardChangeToken(_root, pattern);
            }
            else
            {
                pollingChangeToken = new PollingFileChangeToken(new FileInfo(Path.Combine(_root, pattern)));
            }

            PollingChangeTokens.TryAdd(pollingChangeToken, pollingChangeToken);

            return pollingChangeToken;
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
                    _timer?.Dispose();
                }
                _disposed = true;
            }
        }

        private static string NormalizePath(string filter) => filter = filter.Replace('\\', '/');

        private static bool IsDirectoryPath(string path)
        {
            return path.Length > 0 &&
                (path[path.Length - 1] == Path.DirectorySeparatorChar ||
                path[path.Length - 1] == Path.AltDirectorySeparatorChar);
        }


        internal static void RaiseChangeEvents(object state)
        {
            // Iterating over a concurrent bag gives us a point in time snapshot making it safe
            // to remove items from it.
            var changeTokens = (ConcurrentDictionary<IPollingChangeToken, IPollingChangeToken>)state;
            foreach (System.Collections.Generic.KeyValuePair<IPollingChangeToken, IPollingChangeToken> item in changeTokens)
            {
                IPollingChangeToken token = item.Key;

                if (!token.HasChanged)
                {
                    continue;
                }

                if (!changeTokens.TryRemove(token, out _))
                {
                    // Move on if we couldn't remove the item.
                    continue;
                }

                // We're already on a background thread, don't need to spawn a background Task to cancel the CTS
                try
                {
                    token.CancellationTokenSource.Cancel();
                }
                catch
                {

                }
            }
        }
    }
}
