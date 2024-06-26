﻿using System;
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
        Assert.That(this.Host.WaitForExit(5_000), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(this.Host.ExitCode, Is.Zero);
            Assert.That(this.Host.StandardOutput.ReadToEnd(), Is.Empty);
            Assert.That(this.Host.StandardError.ReadToEnd(), Is.Empty);
        });
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
