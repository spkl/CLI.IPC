// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC.Startup;

/// <summary>
/// Exception type used when a problem occurs in <see cref="SingletonApp"/>.
/// </summary>
public class SingletonAppException : Exception
{
    internal SingletonAppException(string? message) : base(message)
    {
    }
}
