using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
#if NET6_0_OR_GREATER
            yield return new TestCaseData(() => new UdsTransport("someFile")).SetArgDisplayNames(nameof(UdsTransport));
#endif
            yield return new TestCaseData(() => new TcpLoopbackTransport(65057)).SetArgDisplayNames(nameof(TcpLoopbackTransport));
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
        this.host = Host.Start(createTransport(), new ClientConnectionHandler());
        this.WaitForHostStartUp();

        // act & assert
        Assert.That(() => Client.Attach(createTransport(), new HostConnectionHandler()), Throws.Nothing);
        Assert.That(() => this.host.Shutdown(), Throws.Nothing);
    }

    private void WaitForHostStartUp()
    {
        Thread.Sleep(100);
    }

    private class ClientConnectionHandler : IClientConnectionHandler
    {
        public TaskFactory TaskFactory => Task.Factory;

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
