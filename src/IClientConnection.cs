// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System.IO;

namespace spkl.CLI.IPC;

/// <summary>
/// Holds all information and capability to let a host interact with a client.
/// </summary>
public interface IClientConnection
{
    /// <summary>
    /// Gets the information sent by a client.
    /// </summary>
    IClientProperties Properties { get; }

    /// <summary>
    /// Gets the standard output stream.
    /// </summary>
    TextWriter Out { get; }

    /// <summary>
    /// Gets the error output stream.
    /// </summary>
    TextWriter Error { get; }

    /// <summary>
    /// Sends the <paramref name="exitCode"/> to the client and closes the connection.
    /// </summary>
    void Exit(int exitCode);
}
