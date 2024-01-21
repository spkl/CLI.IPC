// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

namespace spkl.CLI.IPC;

/// <summary>
/// Specifies what information a client sends to a host and how it handles the received commands.
/// </summary>
public interface IHostConnectionHandler
{
    /// <summary>
    /// Gets the command line arguments to send.
    /// </summary>
    string[] Arguments { get; }

    /// <summary>
    /// Gets the current working directory to send.
    /// </summary>
    string CurrentDirectory { get; }

    /// <summary>
    /// Called when a standard output string was received.
    /// </summary>
    void HandleOutString(string text);

    /// <summary>
    /// Called when an error output string was received.
    /// </summary>
    void HandleErrorString(string text);

    /// <summary>
    /// Called when the host has closed the connection.
    /// </summary>
    void HandleExit(int exitCode);
}
