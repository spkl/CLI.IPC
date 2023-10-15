using System;
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

    [Test]
    public void CanStartHostOnExistingFile()
    {
        // arrange
        const string fileName = "existingFile";
        File.WriteAllText(fileName, "");

        // act & assert
        Assert.That(() => this.host = Host.Start(fileName, new ClientConnectionHandler()), Throws.Nothing);
    }

    [Test]
    public void HostCanBeInSameProcessAsClient()
    {
        // arrange
        const string fileName = "someFile";
        this.host = Host.Start(fileName, new ClientConnectionHandler());

        // act & assert
        Assert.That(() => Client.Attach(fileName, new HostConnectionHandler()), Throws.Nothing);
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
