// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC;

/// <summary>
/// Exception type used when a connection cannot be established or is interrupted.
/// </summary>
public class ConnectionException : Exception
{
    internal ConnectionException(string message) : base(message)
    {
    }

    internal ConnectionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
