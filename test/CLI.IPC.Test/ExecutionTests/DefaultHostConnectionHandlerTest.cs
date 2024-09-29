using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace spkl.CLI.IPC.Test.ExecutionTests;

#if !NET6_0_OR_GREATER
[Platform(Exclude = "Linux")]
#endif
internal class DefaultHostConnectionHandlerTest : DynamicExecutionTest
{
    [Test]
    public void TestDefaultHostConnectionHandler()
    {
        // act
        this.RunHostAndClient<ClientConnectionHandler, DefaultHostConnectionHandler>();

        // assert
        string expectedExecutable = this.Client.StartInfo.FileName;
#if NET6_0_OR_GREATER
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            expectedExecutable = expectedExecutable.Replace(".exe", ".dll");
        }
        else
        {
            expectedExecutable += ".dll";
        }
#endif

        using (new AssertionScope())
        {
            this.Client.ExitCode.Should().Be(42);
            this.Client.StandardOutput.ReadToEnd().Should().Be(
@$"Arguments: {expectedExecutable},{string.Join(",", this.ClientArguments)}.
CurrentDirectory: {TestContext.CurrentContext.TestDirectory}.
");
            this.Client.StandardError.ReadToEnd().Should().Be(
@"This is an error output.
");
        }
    }

    private class ClientConnectionHandler : IClientConnectionHandler
    {
        public TaskFactory TaskFactory => Task.Factory;

        public void HandleCall(IClientConnection connection)
        {
            connection.Out.WriteLine($"Arguments: {string.Join(",", connection.Properties.Arguments)}.");
            connection.Out.WriteLine($"CurrentDirectory: {connection.Properties.CurrentDirectory}.");
            connection.Error.WriteLine("This is an error output.");
            connection.Exit(42);
        }

        public void HandleListenerError(IListenerError error)
        {
            Console.WriteLine(error.ToString());
            Environment.Exit(1);
        }
    }
}
