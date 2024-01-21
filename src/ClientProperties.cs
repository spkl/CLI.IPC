// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC;

public class ClientProperties
{
    public string[] Arguments { get; set; }

    public string CurrentDirectory { get; set; }

    public ClientProperties()
    {
        this.Arguments = Array.Empty<string>();
        this.CurrentDirectory = string.Empty;
    }
}
