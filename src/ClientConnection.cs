// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using spkl.CLI.IPC.Internal;
using spkl.CLI.IPC.Messaging;
using System.IO;

namespace spkl.CLI.IPC;

/// <inheritdoc cref="IClientConnection"/>
public class ClientConnection : IClientConnection
{
    /// <inheritdoc/>
    public IClientProperties Properties { get; }

    internal MessageChannel Channel { get; }

    internal bool HasExited { get; private set; }

    /// <inheritdoc/>
    public TextWriter Out { get; }

    /// <inheritdoc/>
    public TextWriter Error { get; }

    internal ClientConnection(IClientProperties properties, MessageChannel channel)
    {
        this.Properties = properties;
        this.Channel = channel;
        this.Out = new DelegateTextWriter(this.Channel.Sender.SendOutStr);
        this.Error = new DelegateTextWriter(this.Channel.Sender.SendErrStr);
    }

    /// <inheritdoc/>
    public void Exit(int exitCode)
    {
        this.Channel.Sender.SendExit(exitCode);
        this.Channel.Close();
        this.HasExited = true;
    }
}
