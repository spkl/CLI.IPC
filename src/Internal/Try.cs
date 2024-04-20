// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC.Internal;

internal class Try
{
    public static void UntilTimedOut(TimeSpan timeout, Func<bool> action)
    {
        bool success = false;

        DateTime startTime = DateTime.Now;
        while (!success && (DateTime.Now - startTime) < timeout)
        {
            success = action();
        }
    }

    public static void Dispose<T>(ref T? disposable) where T : IDisposable
    {
        disposable?.Dispose();
        disposable = default;
    }
}
