using spkl.CLI.IPC.Messaging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace spkl.CLI.IPC.Test.Messaging;
internal class MessageReceiverTest : TestBase
{
    public static IEnumerable<TestCaseData> ReceiveMethods
    {
        get
        {
            yield return new TestCaseData(new Action<MessageReceiver>(o => o.ReceiveReqArgs())).SetArgDisplayNames(nameof(MessageReceiver.ReceiveReqArgs));
            yield return new TestCaseData(new Action<MessageReceiver>(o => o.ReceiveReqCurrentDir())).SetArgDisplayNames(nameof(MessageReceiver.ReceiveReqCurrentDir));
            yield return new TestCaseData(new Action<MessageReceiver>(o => o.ReceiveReqProcessID())).SetArgDisplayNames(nameof(MessageReceiver.ReceiveReqProcessID));
            yield return new TestCaseData(new Action<MessageReceiver>(o => o.ReceiveArgs())).SetArgDisplayNames(nameof(MessageReceiver.ReceiveArgs));
            yield return new TestCaseData(new Action<MessageReceiver>(o => o.ReceiveCurrentDir())).SetArgDisplayNames(nameof(MessageReceiver.ReceiveCurrentDir));
            yield return new TestCaseData(new Action<MessageReceiver>(o => o.ReceiveProcessID())).SetArgDisplayNames(nameof(MessageReceiver.ReceiveProcessID));
        }
    }

    [Test]
    [TestCaseSource(nameof(MessageReceiverTest.ReceiveMethods))]
    public void ReceiveMethodsThrowConnectionExceptionIfDifferentMessageTypeWasReceived(Action<MessageReceiver> callReceiveMethod)
    {
        // arrange
        this.PrepareReceiver(out MessageReceiver messageReceiver, out Socket sendSocket, out EndPoint endPoint);
        sendSocket.SendTo(new byte[] { (byte)MessageType.Exit }, endPoint);

        // act & assert
        Invoking(() => callReceiveMethod(messageReceiver)).Should().Throw<ConnectionException>();
    }

    [Test]
    public void ExpectStringCanReceiveZeroLengthString()
    {
        // arrange
        this.PrepareReceiver(out MessageReceiver messageReceiver, out Socket sendSocket, out EndPoint endPoint);
        sendSocket.SendTo(BitConverter.GetBytes(0), endPoint);

        // act
        string result = messageReceiver.ExpectString();

        // assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ExpectBytesThrowsConnectionExceptionIfBytesWereExpectedButNotReceived()
    {
        // arrange
        this.PrepareReceiver(out MessageReceiver messageReceiver, out Socket sendSocket, out EndPoint endPoint);
        sendSocket.SendTo(Array.Empty<byte>(), endPoint);

        // act & assert
        Invoking(() => messageReceiver.ExpectBytes(1)).Should().Throw<ConnectionException>();
    }

    private Socket CreateUdpSocket()
    {
        return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    private EndPoint CreateLoopbackEndpoint()
    {
        return new IPEndPoint(IPAddress.Loopback, 0);
    }

    private void PrepareReceiver(out MessageReceiver messageReceiver, out Socket sendSocket, out EndPoint endPoint)
    {
        endPoint = this.CreateLoopbackEndpoint();

        Socket receiveSocket = this.CreateUdpSocket();
        receiveSocket.Bind(endPoint);
        messageReceiver = new MessageReceiver(receiveSocket);

        sendSocket = this.CreateUdpSocket();
        endPoint = receiveSocket.LocalEndPoint!;
    }
}
