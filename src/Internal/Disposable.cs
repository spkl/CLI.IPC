// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC.Internal;

internal class Disposable : IDisposable
{
    private Action? disposeCallback;

    public Disposable(Action disposeCallback)
    {
        this.disposeCallback = disposeCallback;
    }

    public void Dispose()
    {
        this.disposeCallback?.Invoke();
        this.disposeCallback = null;
    }
}
