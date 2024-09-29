using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace spkl.CLI.IPC.Test;

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

    public static IEnumerable<TestCaseData> TestHostInSameProcessAsClientTestCases
    {
        get
        {
#if NET6_0_OR_GREATER
            yield return new TestCaseData(() => new UdsTransport("someFile"), new HostConnectionHandler()).SetArgDisplayNames(nameof(UdsTransport), nameof(HostConnectionHandler));
            yield return new TestCaseData(() => new UdsTransport("someFile"), new HostConnectionHandler2()).SetArgDisplayNames(nameof(UdsTransport), nameof(HostConnectionHandler2));
#endif
            yield return new TestCaseData(() => new TcpLoopbackTransport(TestBase.GetUnusedPort()), new HostConnectionHandler()).SetArgDisplayNames(nameof(TcpLoopbackTransport), nameof(HostConnectionHandler));
            yield return new TestCaseData(() => new TcpLoopbackTransport(TestBase.GetUnusedPort()), new HostConnectionHandler2()).SetArgDisplayNames(nameof(TcpLoopbackTransport), nameof(HostConnectionHandler2));
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
        Action startHost = () => this.host = Host.Start(new UdsTransport(fileName), new ClientConnectionHandler());
        startHost.Should().NotThrow();
        this.WaitForHostStartUp();
    }
#endif

    [Test]
    [TestCaseSource(nameof(HostTest.TestHostInSameProcessAsClientTestCases))]
    public void TestHostInSameProcessAsClient(Func<ITransport> createTransport, HostConnectionHandler hostConnectionHandler)
    {
        // arrange
        ITransport transport = createTransport();
        ClientConnectionHandler clientConnectionHandler = new();
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
        ClientProperties expectedClientProperties = new()
        {
            Arguments = hostConnectionHandler.Arguments,
            CurrentDirectory = hostConnectionHandler.CurrentDirectory,
            ProcessID = hostConnectionHandler.ExpectedProcessID
        };

        clientConnectionHandler.ReceivedClientProperties.Should().BeEquivalentTo(expectedClientProperties);

        hostConnectionHandler.ReceivedOutString.Should().Be("Out1Out2" + Environment.NewLine);
        hostConnectionHandler.ReceivedErrorString.Should().Be("Error1Error2" + Environment.NewLine);
        hostConnectionHandler.ReceivedExitCode.Should().Be(42);

        connectedClientsBefore.Should().Be(0);
        connectedClientsDuring.Should().Be(1);
        this.host.ConnectedClients.Should().Be(0, "host was shut down");
    }

    [Test]
    public void WaitUntilAllClientsDisconnectedThrowsInvalidOperationExceptionWhenCalledBeforeShutdown()
    {
        // arrange
        ITransport transport = new TcpLoopbackTransport(TestBase.GetUnusedPort());
        this.host = Host.Start(transport, new ClientConnectionHandler());
        this.WaitForHostStartUp();

        // act & assert
        Invoking(() => this.host.WaitUntilAllClientsDisconnected()).Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void WaitUntilAllClientsDisconnectedThrowsInvalidOperationExceptionWhenCalledMoreThanOnce()
    {
        // arrange
        ITransport transport = new TcpLoopbackTransport(TestBase.GetUnusedPort());
        this.host = Host.Start(transport, new ClientConnectionHandler());
        this.WaitForHostStartUp();
        this.host.Shutdown();
        this.host.WaitUntilAllClientsDisconnected();

        // act & assert
        Invoking(() => this.host.WaitUntilAllClientsDisconnected()).Should().Throw<InvalidOperationException>();
    }


    private void WaitForHostStartUp()
    {
        Thread.Sleep(200);
    }

    private class ClientConnectionHandler : IClientConnectionHandler
    {
        public TaskFactory TaskFactory => Task.Factory;

        public void HandleCall(IClientConnection connection)
        {
            this.ReceivedClientProperties = connection.Properties;

            connection.Out.Write(nameof(HostConnectionHandler.BeforeExit));
            Thread.Sleep(200);

            connection.Out.Write('O');
            connection.Out.Write("ut1");
            connection.Error.Write('E');
            connection.Error.Write("rror1");
            connection.Out.WriteLine("Out2");
            connection.Error.WriteLine("Error2");
            connection.Exit(42);
        }

        public void HandleListenerError(IListenerError error)
        {
            Assert.Fail(error.ToString() ?? "Unknown listener error");
        }

        public IClientProperties? ReceivedClientProperties { get; private set; }
    }

    public class HostConnectionHandler : IHostConnectionHandler
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

        public virtual int ExpectedProcessID => -1;
    }

    public class HostConnectionHandler2 : HostConnectionHandler, IHostConnectionHandler2
    {
        public int ProcessID => 42;

        public override int ExpectedProcessID => this.ProcessID;
    }
}
