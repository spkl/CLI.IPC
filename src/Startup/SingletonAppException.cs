// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC.Startup;

/// <summary>
/// Exception type used when a problem occurs in <see cref="SingletonApp"/>.
/// </summary>
public class SingletonAppException : Exception
{
    /// <summary>
    /// Initializes a new instance of this class with a specified error message.
    /// </summary>
    public SingletonAppException(string? message) : base(message)
    {
    }
}
