using spkl.CLI.IPC.Messaging;
using System.Net.Sockets;

namespace spkl.CLI.IPC.Test.Messaging;
internal class MessageSenderTest : TestBase
{
    [Test]
    public void SendBytesThrowsConnectionExceptionForSocketError()
    {
        // arrange
        MessageSender messageSender = new(new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp));

        // act & assert
        Invoking(() => messageSender.SendOutStr("")).Should().Throw<ConnectionException>().WithInnerException<SocketException>();
    }
}
