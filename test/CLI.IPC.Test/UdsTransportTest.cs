#if NET6_0_OR_GREATER
using spkl.CLI.IPC.Internal;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace spkl.CLI.IPC.Test;
internal class UdsTransportTest : TestBase
{
    [Test]
    public void EndPointIsUdsEndPointWithChosenPath()
    {
        // arrange
        UdsTransport t = new("test");

        // act
        EndPoint endPoint = t.EndPoint;

        // assert
        endPoint.Should().BeOfType<UnixDomainSocketEndPoint>();
        endPoint.ToString().Should().Be("test");
    }

    [Test]
    public void SocketIsNotNull()
    {
        // arrange
        UdsTransport t = new("test");

        // act
        Socket socket = t.Socket;

        // assert
        socket.Should().NotBeNull();
    }

    [Test]
    public void CanSerialize()
    {
        // arrange
        UdsTransport transport = new("test");
        using MemoryStream typeStream = new MemoryStream();
        using MemoryStream dataStream = new MemoryStream();

        // act
        Serializer.Write(transport, typeStream, dataStream);
        typeStream.Seek(0, SeekOrigin.Begin);
        dataStream.Seek(0, SeekOrigin.Begin);
        transport = Serializer.Read<UdsTransport>(typeStream, dataStream);

        // assert
        transport.Should().NotBeNull();
        transport.FilePath.Should().Be("test");
    }
}
#endif
