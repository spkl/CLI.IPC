// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

#if NET6_0_OR_GREATER
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace spkl.CLI.IPC;

/// <summary>
/// Establishes a local connection using a Unix Domain Socket.
/// </summary>
public class UdsTransport : ITransport
{
    /// <summary>
    /// Gets the file path used for the socket file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// </summary>
    public UdsTransport(string filePath)
    {
        this.FilePath = filePath;
    }

    /// <inheritdoc/>
    public Socket Socket => new(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

    /// <inheritdoc/>
    public EndPoint EndPoint => new UnixDomainSocketEndPoint(this.FilePath);

    /// <inheritdoc/>
    public virtual void BeforeHostStart()
    {
        File.Delete(this.FilePath);
    }

    /// <inheritdoc/>
    public virtual void AfterHostShutdown()
    {
        File.Delete(this.FilePath);
    }
}
#endif