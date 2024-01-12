using System;
using System.Collections.Generic;
using System.IO;

namespace spkl.IPC.Test;

[TestFixture]
internal class HostTest
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
            yield return new TestCaseData(() => new UdsTransport("someFile")).SetArgDisplayNames(nameof(UdsTransport));
            yield return new TestCaseData(() => new TcpLoopbackTransport(65056)).SetArgDisplayNames(nameof(TcpLoopbackTransport));
        }
    }

    [Test]
    public void CanStartHostOnExistingFile()
    {
        // arrange
        const string fileName = "existingFile";
        File.WriteAllText(fileName, "");

        // act & assert
        Assert.That(() => this.host = Host.Start(new UdsTransport(fileName), new ClientConnectionHandler()), Throws.Nothing);
    }

    [Test]
    [TestCaseSource(nameof(HostTest.Transports))]
    public void HostCanBeInSameProcessAsClient(Func<ITransport> createTransport)
    {
        // arrange
        this.host = Host.Start(createTransport(), new ClientConnectionHandler());

        // act & assert
        Assert.That(() => Client.Attach(createTransport(), new HostConnectionHandler()), Throws.Nothing);
        Assert.That(() => this.host.Shutdown(), Throws.Nothing);
    }

    private class ClientConnectionHandler : IClientConnectionHandler
    {
        public void HandleCall(ClientConnection connection)
        {
            connection.Exit(0);
        }

        public void HandleListenerError(ListenerError error)
        {
            Assert.Fail(error.ToString());
        }
    }

    private class HostConnectionHandler : IHostConnectionHandler
    {
        public string[] Arguments => Array.Empty<string>();

        public string CurrentDirectory => "";

        public void HandleErrorString(string text)
        {
        }

        public void HandleExit(int exitCode)
        {
        }

        public void HandleOutString(string text)
        {
        }
    }
}
