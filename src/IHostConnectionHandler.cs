// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

namespace spkl.CLI.IPC;

public interface IHostConnectionHandler
{
    string[] Arguments { get; }

    string CurrentDirectory { get; }

    void HandleOutString(string text);

    void HandleErrorString(string text);

    void HandleExit(int exitCode);
}
