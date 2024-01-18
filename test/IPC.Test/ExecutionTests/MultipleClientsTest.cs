using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace spkl.IPC.Test.ExecutionTests;

[TestFixture]
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
        Assert.Multiple(() =>
        {
            foreach (Process client in clients) 
            {
                Assert.That(client.ExitCode, Is.EqualTo(0));
                Assert.That(client.StandardOutput.ReadToEnd(), Is.EqualTo("Done"));
                Assert.That(client.StandardError.ReadToEnd(), Is.Empty);
            }
        });
    }

    private class ClientConnectionHandler : IClientConnectionHandler
    {
        public TaskFactory TaskFactory => Task.Factory;

        public void HandleCall(ClientConnection connection)
        {
            Thread.Sleep(2000);
            connection.Out.Write("Done");
            connection.Exit(0);
        }

        public void HandleListenerError(ListenerError error)
        {
            Console.WriteLine(error.ToString());
            Environment.Exit(1);
        }
    }
}
