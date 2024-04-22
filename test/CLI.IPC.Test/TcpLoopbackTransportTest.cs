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
        Assert.That(result, Is.InstanceOf<IPEndPoint>());
        Assert.That(((IPEndPoint)result).Port, Is.EqualTo(23230));
    }

    [Test]
    public void SocketIsNotNull()
    {
        // arrange
        TcpLoopbackTransport t = new(0);

        // act
        Socket result = t.Socket;

        // assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void CanSerialize()
    {
        // arrange
        TcpLoopbackTransport t = new(23230);
        using MemoryStream typeStream = new MemoryStream();
        using MemoryStream dataStream = new MemoryStream();

        // act
        Serializer.Write(t, typeStream, dataStream);
        typeStream.Seek(0, SeekOrigin.Begin);
        dataStream.Seek(0, SeekOrigin.Begin);
        t = Serializer.Read<TcpLoopbackTransport>(typeStream, dataStream);

        // assert
        Assert.That(t, Is.Not.Null);
        Assert.That(t.Port, Is.EqualTo(23230));
    }
}
