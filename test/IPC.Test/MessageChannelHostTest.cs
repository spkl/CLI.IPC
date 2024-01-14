using NSubstitute.ExceptionExtensions;
using spkl.IPC.Messaging;
using spkl.IPC.Services;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace spkl.IPC.Test;
internal class MessageChannelHostTest : TestBase
{
    [Test]
    public void CallsHandleListenerException()
    {
        // arrange
        int port = TestBase.GetUnusedPort();
        ITransport transport = Substitute.For<ITransport>();
        transport.Socket.Returns(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        transport.EndPoint.Returns(new IPEndPoint(IPAddress.Loopback, port));

        Exception exception = new("Foo");
        ServiceProvider.MessageChannelFactory = Substitute.For<IMessageChannelFactory>();
        ServiceProvider.MessageChannelFactory.CreateForIncoming(Arg.Any<Socket>()).Throws(exception);

        MessageChannelHost messageChannelHost = new(transport, Task.Factory, messageChannel => { }, HandleListenerException);
        Exception? receivedException = null;
        void HandleListenerException(Exception exception) => receivedException = exception;

        // act
        messageChannelHost.AcceptConnections();
        new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp).Connect(new IPEndPoint(IPAddress.Loopback, port));
        Thread.Sleep(100);
        
        // assert
        Assert.That(receivedException, Is.EqualTo(exception));
    }
}
