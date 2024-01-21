// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC;

/// <summary>
/// Encapsulates all the information sent by a client.
/// </summary>
public class ClientProperties
{
    /// <summary>
    /// Gets the command line arguments.
    /// </summary>
    public string[] Arguments { get; set; }

    /// <summary>
    /// Gets the current working directory.
    /// </summary>
    public string CurrentDirectory { get; set; }

    /// <summary>
    /// </summary>
    public ClientProperties()
    {
        this.Arguments = Array.Empty<string>();
        this.CurrentDirectory = string.Empty;
    }
}
