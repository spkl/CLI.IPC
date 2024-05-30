// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System.IO;

namespace spkl.CLI.IPC.Internal;

internal class FileStreams
{
    public static FileStream OpenForLocking(string path)
    {
        return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
    }

    public static void OpenForChecking(string path)
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
