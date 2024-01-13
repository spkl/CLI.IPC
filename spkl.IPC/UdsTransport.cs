#if NET6_0_OR_GREATER
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace spkl.IPC;

/// <summary>
/// Establishes a local connection using a Unix Domain Socket.
/// </summary>
public class UdsTransport : ITransport
{
    public string FilePath { get; }

    public UdsTransport(string filePath)
    {
        this.FilePath = filePath;
    }

    public Socket Socket => new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

    public EndPoint EndPoint => new UnixDomainSocketEndPoint(this.FilePath);

    public virtual void BeforeHostStart()
    {
        File.Delete(this.FilePath);
    }

    public virtual void AfterHostShutdown()
    {
        File.Delete(this.FilePath);
    }
}
#endif