// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.IO;

namespace spkl.CLI.IPC.Internal;

internal class FileLock
{
    public string Path { get; }

    private FileStream? stream;

    public FileLock(string path)
    {
        this.Path = path;
    }

    /// <summary>
    /// Obtains the lock. If this fails, an exception is thrown.
    /// See <see cref="FileStream(string, FileMode, FileAccess, FileShare, int, FileOptions)"/> for possible exceptions.
    /// </summary>
    public void Lock()
    {
        this.stream = FileStreams.OpenForLocking(this.Path);
    }

    /// <summary>
    /// Tries to obtain the lock.
    /// </summary>
    /// <returns>Whether the lock was obtained.</returns>
    public bool TryLock()
    {
        try
        {
            this.Lock();
            return true;
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return false;
    }

    /// <summary>
    /// Tries to obtain the lock until it succeeds or <paramref name="timeout"/> is reached.
    /// </summary>
    /// <returns>Whether the lock was obtained.</returns>
    public bool TryLock(TimeSpan timeout)
    {
        bool result = false;
        Try.UntilTimedOut(timeout, () =>
        {
            result = this.TryLock();
            return result;
        });

        return result;
    }

    /// <summary>
    /// Returns whether this instance is holding the lock.
    /// </summary>
    public bool IsHoldingLock()
    {
        return this.stream != null;
    }

    /// <summary>
    /// Returns whether the lock is currently locked.
    /// </summary>
    public bool IsLocked()
    {
        if (!File.Exists(this.Path))
        {
            return false;
        }

        try
        {
            FileStreams.OpenForChecking(this.Path);
            return false;
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return true;
    }

    /// <summary>
    /// Unlocks the lock, if this instance is currently holding the lock.
    /// </summary>
    public void Unlock()
    {
        Try.Dispose(ref this.stream);
    }
}
