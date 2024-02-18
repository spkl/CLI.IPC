// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

namespace spkl.CLI.IPC;

/// <summary>
/// Specifies what information a client sends to a host and how it handles the received commands.
/// This is version 2 of this interface: <see cref="ProcessID"/> was added.
/// </summary>
public interface IHostConnectionHandler2 : IHostConnectionHandler
{
    /// <summary>
    /// Gets the process ID.
    /// </summary>
    int ProcessID { get; }
}
