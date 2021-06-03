// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Primitives;
using symLinkConfig;

namespace Microsoft.Extensions.FileProviders.Physical
{
    /// <summary>
    ///     <para>
    ///     A change token that polls for file system changes.
    ///     </para>
    ///     <para>
    ///     This change token does not raise any change callbacks. Callers should watch for <see cref="HasChanged" /> to turn
    ///     from false to true
    ///     and dispose the token after this happens.
    ///     </para>
    /// </summary>
    /// <remarks>
    /// Polling occurs every 4 seconds.
    /// </remarks>
    public class PollingFileChangeToken : IPollingChangeToken
    {
        private readonly FileInfo _fileInfo;
        private DateTime _previousWriteTimeUtc;
        private DateTime _lastCheckedTimeUtc;
        private bool _hasChanged;
        private CancellationTokenSource _tokenSource;
        private CancellationChangeToken _changeToken;

        /// <summary>
        /// Initializes a new instance of <see cref="PollingFileChangeToken" /> that polls the specified file for changes as
        /// determined by <see cref="System.IO.FileSystemInfo.LastWriteTimeUtc" />.
        /// </summary>
        /// <param name="fileInfo">The <see cref="System.IO.FileInfo"/> to poll</param>
        public PollingFileChangeToken(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
            _previousWriteTimeUtc = GetLastWriteTimeUtc();
            _tokenSource = new CancellationTokenSource();
            _changeToken = new CancellationChangeToken(_tokenSource.Token);
        }

        // Internal for unit testing
        internal static TimeSpan PollingInterval { get; set; } = PollingPhysicalFilesWatcher.DefaultPollingInterval;

        private DateTime GetLastWriteTimeUtc()
        {
            _fileInfo.Refresh();
            return _fileInfo.Exists ? SymlinkHelper.GetSymbolicLinkTargetLastWriteTime(_fileInfo.FullName) : DateTime.MinValue;
        }

        public bool ActiveChangeCallbacks => true;

        CancellationTokenSource IPollingChangeToken.CancellationTokenSource => _tokenSource;

        /// <summary>
        /// True when the file has changed since the change token was created. Once the file changes, this value is always true
        /// </summary>
        /// <remarks>
        /// Once true, the value will always be true. Change tokens should not re-used once expired. The caller should discard this
        /// instance once it sees <see cref="HasChanged" /> is true.
        /// </remarks>
        public bool HasChanged
        {
            get
            {
                if (_hasChanged)
                {
                    return _hasChanged;
                }

                DateTime currentTime = DateTime.UtcNow;
                if (currentTime - _lastCheckedTimeUtc < PollingInterval)
                {
                    return _hasChanged;
                }

                DateTime lastWriteTimeUtc = GetLastWriteTimeUtc();
                if (_previousWriteTimeUtc != lastWriteTimeUtc)
                {
                    _previousWriteTimeUtc = lastWriteTimeUtc;
                    _hasChanged = true;
                }

                _lastCheckedTimeUtc = currentTime;
                return _hasChanged;
            }
        }

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            return _changeToken.RegisterChangeCallback(callback, state);
        }
    }
}
