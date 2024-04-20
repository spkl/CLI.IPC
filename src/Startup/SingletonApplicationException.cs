// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC.Startup;

/// <summary>
/// Exception type used when a problem occurs in <see cref="SingletonApplication"/>.
/// </summary>
public class SingletonApplicationException : Exception
{
    /// <summary>
    /// Initializes a new instance of this class with a specified error message.
    /// </summary>
    public SingletonApplicationException(string? message) : base(message)
    {
    }
}
