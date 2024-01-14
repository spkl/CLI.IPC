using spkl.IPC.Messaging;
using spkl.IPC.Services;
using System.Net.Sockets;

namespace spkl.IPC.Test;

internal class ClientTest : TestBase
{
    private const int InvalidPort = 0;
    private const int UnservicedPort = 65064;

    [Test]
    [TestCase(ClientTest.InvalidPort)]
    [TestCase(ClientTest.UnservicedPort)]
    public void AttachThrowsConnectionExceptionIfNoConnectionCanBeEstablished(int port)
    {
        // arrange
        ITransport transport = new TcpLoopbackTransport(port);
        IHostConnectionHandler hostConnectionHandler = Substitute.For<IHostConnectionHandler>();

        // act & assert
        Assert.That(() => Client.Attach(transport, hostConnectionHandler), Throws.InstanceOf<ConnectionException>().With.Message.Contains("Could not connect"));
    }

    [Test]
    public void AttachThrowsConnectionExceptionIfConnectionIsInterrupted()
    {
        // arrange
        ITransport transport = Substitute.For<ITransport>();
        IHostConnectionHandler hostConnectionHandler = Substitute.For<IHostConnectionHandler>();
        MessageChannel messageChannel = new(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        ServiceProvider.MessageChannelFactory = Substitute.For<IMessageChannelFactory>();
        ServiceProvider.MessageChannelFactory.CreateForOutgoing(transport).Returns(messageChannel);

        // act & assert
        Assert.That(() => Client.Attach(transport, hostConnectionHandler), Throws.InstanceOf<ConnectionException>().With.Message.Contains("There was an unexpected connection error"));
    }
}
