using System;
using System.Threading.Tasks;

namespace spkl.CLI.IPC.Test.ExecutionTests;

#if !NET6_0_OR_GREATER
[Platform(Exclude = "Linux")]
#endif
internal class HostShutdownTest : DynamicExecutionTest
{
    [Test]
    public void TestHostShutdown()
    {
        // act
        this.RunHostAndClient<ClientConnectionHandler, DefaultHostConnectionHandler>();
        this.WaitForClientExit();
        Assume.That(this.Host.HasExited, Is.False, "Host has exited");
        this.Host.StandardInput.WriteLine();

        // assert
        this.Host.WaitForExit(5_000).Should().BeTrue();
        using (new AssertionScope())
        {
            this.Host.ExitCode.Should().Be(0);
            this.Host.StandardOutput.ReadToEnd().Should().BeEmpty();
            this.Host.StandardError.ReadToEnd().Should().BeEmpty();
        }
    }

    private class ClientConnectionHandler : IClientConnectionHandler
    {
        public TaskFactory TaskFactory => Task.Factory;

        public void HandleCall(IClientConnection connection)
        {
            connection.Exit(0);
        }

        public void HandleListenerError(IListenerError error)
        {
            Console.WriteLine(error.ToString());
            Environment.Exit(1);
        }
    }
}
