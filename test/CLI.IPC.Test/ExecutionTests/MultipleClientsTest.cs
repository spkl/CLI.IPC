using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace spkl.CLI.IPC.Test.ExecutionTests;

#if !NET6_0_OR_GREATER
[Platform(Exclude = "Linux")]
#endif
internal class MultipleClientsTest : DynamicExecutionTest
{
    [Test]
    public void TestMultipleClients()
    {
        // act
        this.StartHost<ClientConnectionHandler>();
        Process[] clients =
        {
            this.StartClient<DefaultHostConnectionHandler>(),
            this.StartClient<DefaultHostConnectionHandler>(),
            this.StartClient<DefaultHostConnectionHandler>(),
        };

        foreach (Process client in clients)
        {
            this.WaitForClientExit(client);
        }

        // assert
        using (new AssertionScope())
        {
            foreach (Process client in clients)
            {
                client.ExitCode.Should().Be(0);
                client.StandardOutput.ReadToEnd().Should().Be("Done");
                client.StandardError.ReadToEnd().Should().BeEmpty();
            }
        }
    }

    private class ClientConnectionHandler : IClientConnectionHandler
    {
        public TaskFactory TaskFactory => Task.Factory;

        public void HandleCall(IClientConnection connection)
        {
            Thread.Sleep(2000);
            connection.Out.Write("Done");
            connection.Exit(0);
        }

        public void HandleListenerError(IListenerError error)
        {
            Console.WriteLine(error.ToString());
            Environment.Exit(1);
        }
    }
}
