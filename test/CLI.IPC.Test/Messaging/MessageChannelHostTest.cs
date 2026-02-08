using NSubstitute.ExceptionExtensions;
using spkl.CLI.IPC.Messaging;
using spkl.CLI.IPC.Services;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace spkl.CLI.IPC.Test.Messaging;

internal class MessageChannelHostTest : TestBase
{
    private MessageChannelHost? messageChannelHost;

    [TearDown]
    public void TearDown()
    {
        this.messageChannelHost?.Shutdown();
    }

    [Test]
    public void CallsHandleListenerExceptionForConnectionAccept()
    {
        // arrange
        int port = TestBase.GetUnusedPort();
        ITransport transport = Substitute.For<ITransport>();
        transport.Socket.Returns(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        transport.EndPoint.Returns(new IPEndPoint(IPAddress.Loopback, port));

        Exception exception = new("Foo");
        ServiceProvider.MessageChannelFactory = Substitute.For<IMessageChannelFactory>();
        ServiceProvider.MessageChannelFactory.CreateForIncoming(Arg.Any<Socket>()).Throws(exception);

        EventWaitHandle waitHandle = new(false, EventResetMode.ManualReset);

        this.messageChannelHost = new(transport, Task.Factory, messageChannel => { }, HandleListenerException);
        Exception? receivedException = null;
        ListenerErrorPoint? receivedErrorPoint = null;
        void HandleListenerException(Exception exception, ListenerErrorPoint errorPoint)
        {
            receivedException = exception;
            receivedErrorPoint = errorPoint;
            waitHandle.Set();
        }

        // act
        this.messageChannelHost.AcceptConnections();
        new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp).Connect(new IPEndPoint(IPAddress.Loopback, port));
        bool timeout = !waitHandle.WaitOne(TimeSpan.FromSeconds(5));

        // assert
        Assert.That(timeout, Is.False, "Timeout");
        Assert.That(receivedException, Is.EqualTo(exception));
        Assert.That(receivedErrorPoint, Is.EqualTo(ListenerErrorPoint.ConnectionAccept));
    }

    [Test]
    public void CallsHandleListenerExceptionForClientConnectionHandler()
    {
        // arrange
        int port = TestBase.GetUnusedPort();
        ITransport transport = Substitute.For<ITransport>();
        transport.Socket.Returns(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        transport.EndPoint.Returns(new IPEndPoint(IPAddress.Loopback, port));

        EventWaitHandle waitHandle = new(false, EventResetMode.ManualReset);

        Exception exception = new("Foo");
        this.messageChannelHost = new(transport, Task.Factory, HandleNewConnection, HandleListenerException);

        void HandleNewConnection(MessageChannel channel)
        {
            throw exception;
        }

        Exception? receivedException = null;
        ListenerErrorPoint? receivedErrorPoint = null;
        void HandleListenerException(Exception exception, ListenerErrorPoint errorPoint)
        {
            receivedException = exception;
            receivedErrorPoint = errorPoint;
            waitHandle.Set();
        }

        // act
        this.messageChannelHost.AcceptConnections();
        new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp).Connect(new IPEndPoint(IPAddress.Loopback, port));
        bool timeout = !waitHandle.WaitOne(TimeSpan.FromSeconds(5));

        // assert
        Assert.That(timeout, Is.False, "Timeout");
        Assert.That(receivedException, Is.EqualTo(exception));
        Assert.That(receivedErrorPoint, Is.EqualTo(ListenerErrorPoint.ClientConnectionHandler));
    }
}
