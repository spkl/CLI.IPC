// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC;

/// <summary>
/// Exception type used when a connection cannot be established or is interrupted.
/// </summary>
public class ConnectionException : Exception
{
    /// <summary>
    /// Initializes a new instance of this class with a specified error message.
    /// </summary>
    public ConnectionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    public ConnectionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
