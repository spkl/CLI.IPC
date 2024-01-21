// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC.Startup;

public class SingletonApplicationException : Exception
{
    public SingletonApplicationException(string? message) : base(message)
    {
    }
}
