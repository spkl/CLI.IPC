// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

namespace spkl.CLI.IPC;

/// <summary>
/// Encapsulates all the information sent by a client.
/// </summary>
public interface IClientProperties
{
    /// <summary>
    /// Gets the command line arguments.
    /// </summary>
    string[] Arguments { get; set; }

    /// <summary>
    /// Gets the current working directory.
    /// </summary>
    string CurrentDirectory { get; set; }

    /// <summary>
    /// Gets the process ID.
    /// </summary>
    /// <remarks>
    /// If the client does not support this property, -1 is returned.
    /// </remarks>
    int ProcessID { get; set; }
}
