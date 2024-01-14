using spkl.IPC.Messaging;
using System.Net.Sockets;

namespace spkl.IPC.Test;
internal class MessageSenderTest : TestBase
{
    [Test]
    public void SendBytesThrowsConnectionExceptionForSocketError()
    {
        // arrange
        MessageSender messageSender = new(new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp));
        
        // act & assert
        Assert.That(() => messageSender.SendOutStr(""), Throws.InstanceOf<ConnectionException>().With.InnerException.InstanceOf<SocketException>());
    }
}
