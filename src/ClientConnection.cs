// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using spkl.CLI.IPC.Internal;
using spkl.CLI.IPC.Messaging;
using System.IO;

namespace spkl.CLI.IPC;

/// <summary>
/// Holds all information and capability to let a host interact with a client.
/// </summary>
public class ClientConnection
{
    /// <summary>
    /// Gets the information sent by a client.
    /// </summary>
    public ClientProperties Properties { get; }

    internal MessageChannel Channel { get; }

    internal bool HasExited { get; private set; }

    /// <summary>
    /// Gets the standard output stream.
    /// </summary>
    public TextWriter Out { get; }

    /// <summary>
    /// Gets the error output stream.
    /// </summary>
    public TextWriter Error { get; }

    internal ClientConnection(ClientProperties properties, MessageChannel channel)
    {
        this.Properties = properties;
        this.Channel = channel;
        this.Out = new DelegateTextWriter(this.Channel.Sender.SendOutStr);
        this.Error = new DelegateTextWriter(this.Channel.Sender.SendErrStr);
    }

    /// <summary>
    /// Sends the <paramref name="exitCode"/> to the client and closes the connection.
    /// </summary>
    public void Exit(int exitCode)
    {
        this.Channel.Sender.SendExit(exitCode);
        this.Channel.Close();
        this.HasExited = true;
    }
}
