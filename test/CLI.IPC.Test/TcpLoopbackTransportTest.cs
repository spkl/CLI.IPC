using spkl.CLI.IPC.Internal;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace spkl.CLI.IPC.Test;
internal class TcpLoopbackTransportTest : TestBase
{
    [Test]
    public void EndPointIsIPEndPointWithChosenPort()
    {
        // arrange
        TcpLoopbackTransport t = new(23230);

        // act
        EndPoint result = t.EndPoint;

        // assert
        result.Should().BeOfType<IPEndPoint>();
        result.As<IPEndPoint>().Port.Should().Be(23230);
    }

    [Test]
    public void SocketIsNotNull()
    {
        // arrange
        TcpLoopbackTransport t = new(0);

        // act
        Socket socket = t.Socket;

        // assert
        socket.Should().NotBeNull();
    }

    [Test]
    public void CanSerialize()
    {
        // arrange
        TcpLoopbackTransport transport = new(23230);
        using MemoryStream typeStream = new MemoryStream();
        using MemoryStream dataStream = new MemoryStream();

        // act
        Serializer.Write(transport, typeStream, dataStream);
        typeStream.Seek(0, SeekOrigin.Begin);
        dataStream.Seek(0, SeekOrigin.Begin);
        transport = Serializer.Read<TcpLoopbackTransport>(typeStream, dataStream);

        // assert
        transport.Should().NotBeNull();
        transport.Port.Should().Be(23230);
    }
}
