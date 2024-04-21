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
        EndPoint result = t.EndPoint;

        // assert
        Assert.That(result, Is.InstanceOf<UnixDomainSocketEndPoint>());
        Assert.That(((UnixDomainSocketEndPoint)result).ToString(), Is.EqualTo("test"));
    }

    [Test]
    public void SocketIsNotNull()
    {
        // arrange
        UdsTransport t = new("test");

        // act
        Socket result = t.Socket;

        // assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void CanSerialize()
    {
        // arrange
        UdsTransport t = new("test");
        using MemoryStream typeStream = new MemoryStream();
        using MemoryStream dataStream = new MemoryStream();

        // act
        Serializer.Write(t, typeStream, dataStream);
        typeStream.Seek(0, SeekOrigin.Begin);
        dataStream.Seek(0, SeekOrigin.Begin);
        t = Serializer.Read<UdsTransport>(typeStream, dataStream);

        // assert
        Assert.That(t, Is.Not.Null);
        Assert.That(t.FilePath, Is.EqualTo("test"));
    }
}
#endif
