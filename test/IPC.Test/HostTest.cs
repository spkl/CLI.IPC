using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace spkl.IPC.Test;

internal class HostTest : TestBase
{
    private Host? host;

    [TearDown]
    public void TearDown()
    {
        if (this.host != null)
        {
            this.host.Shutdown();
            this.host = null;
        }
    }

    public static IEnumerable<TestCaseData> Transports
    {
        get
        {
#if NET6_0_OR_GREATER
            yield return new TestCaseData(() => new UdsTransport("someFile")).SetArgDisplayNames(nameof(UdsTransport));
#endif
            yield return new TestCaseData(() => new TcpLoopbackTransport(TestBase.GetUnusedPort())).SetArgDisplayNames(nameof(TcpLoopbackTransport));
        }
    }

#if NET6_0_OR_GREATER
    [Test]
    public void CanStartHostOnExistingFile()
    {
        // arrange
        const string fileName = "existingFile";
        File.WriteAllText(fileName, "");

        // act & assert
        Assert.That(() => this.host = Host.Start(new UdsTransport(fileName), new ClientConnectionHandler()), Throws.Nothing);
        this.WaitForHostStartUp();
    }
#endif

    [Test]
    [TestCaseSource(nameof(HostTest.Transports))]
    public void HostCanBeInSameProcessAsClient(Func<ITransport> createTransport)
    {
        // arrange
        ITransport transport = createTransport();
        ClientConnectionHandler clientConnectionHandler = new();
        HostConnectionHandler hostConnectionHandler = new();
        this.host = Host.Start(transport, clientConnectionHandler);
        this.WaitForHostStartUp();
        int connectedClientsBefore = this.host.ConnectedClients;
        int connectedClientsDuring = -1;
        hostConnectionHandler.BeforeExit += (_, _) => connectedClientsDuring = this.host.ConnectedClients;

        // act
        Client.Attach(transport, hostConnectionHandler);
        this.host.Shutdown();
        this.host.WaitUntilAllClientsDisconnected();

        // assert
        Assert.That(clientConnectionHandler.ReceivedClientProperties, Is.Not.Null);
        Assert.That(clientConnectionHandler.ReceivedClientProperties.Arguments, Is.EqualTo(hostConnectionHandler.Arguments));
        Assert.That(clientConnectionHandler.ReceivedClientProperties.CurrentDirectory, Is.EqualTo(hostConnectionHandler.CurrentDirectory));

        Assert.That(hostConnectionHandler.ReceivedOutString, Is.EqualTo("Out1Out2" + Environment.NewLine));
        Assert.That(hostConnectionHandler.ReceivedErrorString, Is.EqualTo("Error1Error2" + Environment.NewLine));
        Assert.That(hostConnectionHandler.ReceivedExitCode, Is.EqualTo(42));

        Assert.That(connectedClientsBefore, Is.EqualTo(0), "Number of connected clients before connecting");
        Assert.That(connectedClientsDuring, Is.EqualTo(1), "Number of connected clients during connection");
        Assert.That(this.host.ConnectedClients, Is.EqualTo(0), "Number of connected clients after shutdown");
    }

    private void WaitForHostStartUp()
    {
        Thread.Sleep(200);
    }

    private class ClientConnectionHandler : IClientConnectionHandler
    {
        public TaskFactory TaskFactory => Task.Factory;

        public void HandleCall(ClientConnection connection)
        {
            this.ReceivedClientProperties = connection.Properties;

            connection.Out.Write(nameof(HostConnectionHandler.BeforeExit));
            connection.Out.Write('O');
            connection.Out.Write("ut1");
            connection.Error.Write('E');
            connection.Error.Write("rror1");
            connection.Out.WriteLine("Out2");
            connection.Error.WriteLine("Error2");
            connection.Exit(42);
        }

        public void HandleListenerError(ListenerError error)
        {
            Assert.Fail(error.ToString());
        }

        public ClientProperties? ReceivedClientProperties { get; private set; }
    }

    private class HostConnectionHandler : IHostConnectionHandler
    {
        public string[] Arguments => new[] { "Foo", "Bar" };

        public string CurrentDirectory => "Baz";

        public void HandleOutString(string text)
        {
            if (text == nameof(HostConnectionHandler.BeforeExit))
            {
                this.BeforeExit?.Invoke(this, EventArgs.Empty);
                return;
            }

            this.ReceivedOutString += text;
        }

        public void HandleErrorString(string text)
        {
            this.ReceivedErrorString += text;
        }

        public void HandleExit(int exitCode)
        {
            this.ReceivedExitCode = exitCode;
        }

        public string ReceivedOutString { get; private set; } = "";

        public string ReceivedErrorString { get; private set; } = "";

        public int? ReceivedExitCode { get; private set; }

        public event EventHandler? BeforeExit;
    }
}
