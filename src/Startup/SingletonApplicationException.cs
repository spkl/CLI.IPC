// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC.Startup;

/// <summary>
/// Exception type used when a problem occurs in <see cref="SingletonApplication"/>.
/// </summary>
public class SingletonApplicationException : Exception
{
    internal SingletonApplicationException(string? message) : base(message)
    {
    }
}
