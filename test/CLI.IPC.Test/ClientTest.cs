using spkl.CLI.IPC.Messaging;
using spkl.CLI.IPC.Services;
using System;
using System.Net.Sockets;

namespace spkl.CLI.IPC.Test;

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
        Action attach = () => Client.Attach(transport, hostConnectionHandler);
        attach.Should().Throw<ConnectionException>().WithMessage("Could not connect*");
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
        Action attach = () => Client.Attach(transport, hostConnectionHandler);
        attach.Should().Throw<ConnectionException>().WithMessage("There was an unexpected connection error*");
    }
}
