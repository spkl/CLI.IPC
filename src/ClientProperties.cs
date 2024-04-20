// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC;

/// <inheritdoc cref="IClientProperties"/>
public class ClientProperties : IClientProperties
{
    /// <inheritdoc/>
    public string[] Arguments { get; set; }

    /// <inheritdoc/>
    public string CurrentDirectory { get; set; }

    /// <inheritdoc/>
    public int ProcessID { get; set; }

    /// <summary>
    /// </summary>
    public ClientProperties()
    {
        this.Arguments = Array.Empty<string>();
        this.CurrentDirectory = string.Empty;
        this.ProcessID = -1;
    }
}
