// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.IO;

namespace spkl.CLI.IPC.Internal;

internal class FileStreams
{
    public static FileStream? TryLock(string path)
    {
        try
        {
            return FileStreams.OpenForLocking(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return null;
    }

    public static FileStream? TryLock(string path, TimeSpan timeout)
    {
        FileStream? stream = null;
        Try.UntilTimedOut(timeout, () =>
        {
            stream = FileStreams.TryLock(path);
            return stream != null;
        });

        return stream;
    }

    public static bool IsLocked(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            FileStreams.OpenForChecking(path);
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

    public static FileStream OpenForLocking(string path)
    {
        return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
    }

    private static void OpenForChecking(string path)
    {
        new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete, 4096, FileOptions.DeleteOnClose).Dispose();
    }

    public static FileStream OpenForExclusiveWriting(string path)
    {
        return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.DeleteOnClose);
    }

    public static FileStream OpenForSharedReading(string path)
    {
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 4096);
    }
}
