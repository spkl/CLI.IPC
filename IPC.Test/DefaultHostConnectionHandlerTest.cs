using System;

namespace spkl.IPC.Test;

[TestFixture]
public class DefaultHostConnectionHandlerTest : DynamicTest
{
    [Test]
    public void Test()
    {
        // act
        this.RunServerAndClient<ClientConnectionHandler, DefaultHostConnectionHandler>();

        // assert
        Assert.That(this.Client.ExitCode, Is.EqualTo(42));
        Assert.That(this.Client.StandardOutput.ReadToEnd(), Is.EqualTo(
@$"Arguments: {this.Client.StartInfo.FileName.Replace(".exe", ".dll")},{string.Join(",", this.Client.StartInfo.ArgumentList)}.
CurrentDirectory: {TestContext.CurrentContext.TestDirectory}.
"));
        Assert.That(this.Client.StandardError.ReadToEnd(), Is.EqualTo(
@"This is an error output.
"));
    }

    private class ClientConnectionHandler : IClientConnectionHandler
    {
        public void HandleCall(ClientConnection connection)
        {
            connection.Out.WriteLine($"Arguments: {string.Join(",", connection.Properties.Arguments)}.");
            connection.Out.WriteLine($"CurrentDirectory: {connection.Properties.CurrentDirectory}.");
            connection.Error.WriteLine("This is an error output.");
            connection.Exit(42);
        }

        public void HandleListenerError(ListenerError error)
        {
            Console.WriteLine(error.ToString());
            Environment.Exit(1);
        }
    }
}
